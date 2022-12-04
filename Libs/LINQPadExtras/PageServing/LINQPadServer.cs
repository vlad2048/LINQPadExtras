using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.PageLogic;
using LINQPadExtras.PageServing.PageLogic.Transformers;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
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

	internal static bool IsStarted { get; private set; }
	internal static Disp MasterD => serD.Value!;
	internal static IObservable<Chg> WhenChg { get; private set; } = null!;
	internal static Tweaks Tweaks { get; private set; } = null!;
	internal static void SignalRefreshNeeded() => whenRefreshNeeded.OnNext(Unit.Default);

	public static void AddLINQPadOnlyCss(string css)
	{
		if (css.Contains(Environment.NewLine))
			css = css.Replace(Environment.NewLine, "\n");
		if (cssContentsToRemove.Contains(css)) return;
		Util.HtmlHead.AddStyles(css);
		cssContentsToRemove.Add(css);
	}

	internal static ITransformer[] GetSaveTransformers() => new ITransformer[]
	{
		new RemoveCssScriptTransformer(cssContentsToRemove),
	};


	public static void Start(Action<LINQPadServerOpt>? optFun = null)
	{
		var opt = LINQPadServerOpt.Build(optFun);
		serD.Value = null;
		var d = serD.Value = new Disp();
		whenRefreshNeeded = new Subject<Unit>().D(d);
		cssContentsToRemove.Clear();
		Tweaks = new Tweaks();

		var replier = new ServerReplier();
		var pageMutator = new PageMutator(

			new BodyWrapperTransformer(body => $"""
				{FrontendScripts.HtmlRefreshUI}
				<div id="{FrontendScripts.MainDivId}">
					{body}
				</div>
				{FrontendScripts.JSNotifyLogic}
				"""),

			new RemoveNodeTransformer(FrontendScripts.ServerConnectionId),

			new ApplyTweaksTransformer(Tweaks),

			new ImageFixerTransformer(replier),

			new RemoveCssScriptTransformer(cssContentsToRemove),
			
			new CssScriptExtractionTransformer(replier, o =>
			{
				o.PredefinedNames.AddRange(new[] { "linqpad", "reset" });
				//o.GroupRestInLastName = true;
			}),

			new JsScriptExtractionTransformer(replier, o =>
			{
				o.PredefinedNames.Add("linqpad");
			}),

			new AddJsScriptTransformer(replier, "refresh", FrontendScripts.JSRefreshLogic(opt.WsPort))

		);

		replier.HtmlPageFun = pageMutator.GetPage;

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

		var pageUrl = $"http://{FrontendScripts.MachineName}:{opt.HttpPort}/";
		MakeConnectionUI(server.WSState, pageUrl, opt.HtmlEditFolder, pageMutator, replier).Dump();
		IsStarted = true;
	}

	private static Control MakeConnectionUI(
		IRoVar<WSState> wsState,
		string pageUrl,
		string? htmlEditFolder,
		PageMutator pageMutator,
		ServerReplier replier
	)
	{
		Util.HtmlHead.AddStyles("""
			.connection {
				display: flex;
				align-items: baseline;
				column-gap: 5px;
				background-color: #333;
			}
			.connection-link {
				margin-left: auto;
			}
			""");

		DumpContainer dcStatus, dcClose, dcError;
		TextBox folderText;

		var div = new Div(
			new Span("Server connection:"),
			dcStatus = new DumpContainer(),
			dcClose = new DumpContainer(),
			dcError = new DumpContainer(),
			new Label(pageUrl)
			{
				CssClass = "connection-link",
			},
			folderText = new TextBox(htmlEditFolder ?? string.Empty),
			new Button("Edit html", _ => PageSaver.Save(folderText.Text, pageMutator, replier))
		)
		{
			CssClass = "connection",
			HtmlElement =
			{
				ID = FrontendScripts.ServerConnectionId
			}
		};

		wsState.Subscribe(state =>
		{
			dcStatus.UpdateContent($"{state.Status}");
			dcClose.UpdateContent(state.CloseNfo.IsSome(out var close) switch
			{
				true => $"code:{close!.Code} wasClean:{close.WasClean} reason:'{close.Reason}'",
				false => string.Empty
			});
			dcError.UpdateContent(state.ErrorNfo.IsSome(out var err) switch
			{
				true => $"err:{err!.Message} ex:{err.Ex.Message}",
				false => string.Empty
			});
		}).D(wsState);

		return div;
	}
}