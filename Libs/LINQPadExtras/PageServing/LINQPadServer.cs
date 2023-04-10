using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using LINQPad;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.ConnUIClientServer;
using LINQPadExtras.PageServing.PageLogic;
using LINQPadExtras.PageServing.PageLogic.Transformers;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Replying;
using LINQPadExtras.PageServing.Structs;
using LINQPadExtras.PageServing.Utils;
using PowMaybe;
using PowRxVar;

namespace LINQPadExtras.PageServing;

public class LINQPadServerOpt
{
	public int HttpPort { get; set; } = 6240;
	public int WsPort { get; set; } = 9730;
	public string? HtmlEditFolder { get; set; }
	public bool FullScreenLayout { get; set; }
	public List<ITransformer> Transformers { get; } = new();
	public List<IReplier> Repliers { get; } = new();

	internal static LINQPadServerOpt Build(Action<LINQPadServerOpt>? optFun)
	{
		var opt = new LINQPadServerOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

public static class LINQPadServer
{
	private static readonly SerialDisp<Disp> serD = new();
	private static ISubject<Unit> whenRefreshNeeded = null!;
	private static IObservable<Unit> WhenRefreshNeeded => whenRefreshNeeded.AsObservable();
	private static readonly HashSet<string> cssContentsToRemove = new();
	private static AddCssScriptTransformer addCssScriptTransformer = null!;

	internal static bool IsStarted { get; private set; }
	internal static Disp MasterD => serD.Value!;
	internal static IObservable<Chg> WhenChg { get; private set; } = null!;
	internal static Tweaks Tweaks { get; private set; } = null!;
	internal static void SignalRefreshNeeded() => whenRefreshNeeded.OnNext(Unit.Default);

	internal static ITransformer[] GetSaveTransformers() => new ITransformer[]
	{
		new RemoveCssScriptTransformer(cssContentsToRemove),
	};
	

	public static void AddServerCss(string css)
	{
		if (css.Contains(Environment.NewLine))
			css = css.Replace(Environment.NewLine, "\n");
		if (cssContentsToRemove.Contains(css)) return;
		Util.HtmlHead.AddStyles(css);
		cssContentsToRemove.Add(css);
	}

	//public static void AddClientCss(string scriptName, string css) => addHeadLinkTransformer.AddClientCss(scriptName, css);
	public static void AddClientCss(string scriptName, string css) => addCssScriptTransformer.AddClientCss(scriptName, css);


	public static void Start(Action<LINQPadServerOpt>? optFun = null)
	{
		var opt = LINQPadServerOpt.Build(optFun);
		serD.Value = null;
		var d = serD.Value = new Disp();

		whenRefreshNeeded = new Subject<Unit>().D(d);
		cssContentsToRemove.Clear();
		Tweaks = new Tweaks();

		var contentHolder = new ReplierContentHolder();
		addCssScriptTransformer = new AddCssScriptTransformer(contentHolder);
		var pageMutator = new PageMutator(
			Arr(
					ConnUI.Client_BodyWrapper(),
					new RemoveNodeTransformer(ConnUI.ConnServerId),
					new ApplyTweaksTransformer(Tweaks),
					new ImageFixerTransformer(contentHolder),
					new RemoveCssScriptTransformer(cssContentsToRemove),
					new CssScriptExtractionTransformer(contentHolder, o =>
					{
						o.PredefinedNames.AddRange(new[] { "linqpad", "reset" });
						//o.GroupRestInLastName = true;
					}),
					new JsScriptExtractionTransformer(contentHolder, o =>
					{
						o.PredefinedNames.Add("linqpad");
					}),
					new AddJsScriptTransformer(contentHolder, "refresh", FrontendScripts.JSRefreshLogic(opt.WsPort)),
					addCssScriptTransformer
				)
				.Concat(opt.Transformers)
				.ToArray()
		);
		var replierCtx = new ReplierCtx(opt.HttpPort);
		opt.Repliers.ForEach(e => e.AssignCtx(replierCtx));
		var replier = new Replier(contentHolder, pageMutator, opt.Repliers.ToArray());

		var server = new Server(replier, opt.HttpPort, opt.WsPort).D(d);
		WhenChg = server.WhenChg;

		var refreshPrev = May.None<PageRefreshNfo>();
		var lastRefreshTime = DateTime.MinValue;
		var isRefreshing = false;

		var gate = new object();

		WhenRefreshNeeded
			.Throttle(PageServingConsts.RefreshDelay)
			.Synchronize(gate)
			.Subscribe(_ =>
			{
				var refresh = pageMutator.GetPageRefresh();
				var refreshNext = May.Some(refresh);
				if (refreshNext == refreshPrev) return;

				isRefreshing = true;
				lastRefreshTime = DateTime.Now;
				refreshPrev = refreshNext;

				var str = refresh.Ser();
				server.SendData(str);
				isRefreshing = false;
			}).D(d);

		Observable.Interval(PageServingConsts.MissedRefreshInterval)
			.Where(_ => !isRefreshing)
			.Where(_ => DateTime.Now - lastRefreshTime > PageServingConsts.MissedRefreshWait)
			.Subscribe(_ =>
			{
				SignalRefreshNeeded();
			}).D(d);

		server.Start();

		ConnUI.Server_Dump(server.WSState, opt.HtmlEditFolder, opt.HttpPort, pageMutator, contentHolder);
		AddCss(opt.FullScreenLayout);
		IsStarted = true;
	}


	private static void AddCss(bool fullScreenLayout)
	{
		// Sensible reset for server & client
		Util.HtmlHead.AddStyles("""
			*, *::before, *::after {
				box-sizing: border-box;
			}
			* {
				margin: 0 !important;
				padding: 0;
				border: 0;
			}
			*, *::before, *::after {
				box-sizing: border-box;
			}
			* {
				margin: 0 !important;
				padding: 0;
				border: 0;
			}
			""");

		// Correct the mysterious 2px missing in LINQPad
		AddServerCss("""
			body {
				padding: 2px;
			}
			""");

		if (fullScreenLayout)
		{
			AddServerCss("""
				html, body {
					height: 100%;
				}
				#final {
					height: calc(100% - 25px);
				}
				""");

			AddClientCss("fullscreen", """
				html {
					height: 100%;
				}
				body {
					height: calc(100% - 25px);
				}
				#final {
					height: 100%;
				}
				""");
		}
	}



	private static ITransformer[] Arr(params ITransformer[] arr) => arr;
}