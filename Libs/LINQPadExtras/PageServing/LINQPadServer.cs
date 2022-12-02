using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.Structs;
using LINQPadExtras.PageServing.Utils;
using LINQPadExtras.PageServing.Utils.HtmlUtils;
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

	internal static Disp MasterD => serD.Value!;
	internal static IObservable<Chg> WhenChg { get; private set; } = null!;
	internal static Tweaks Tweaks { get; private set; } = null!;
	internal static void SignalRefreshNeeded() => whenRefreshNeeded.OnNext(Unit.Default);

	private record RefreshRec(
		string HtmlStyle,
		string Head,
		string Body
	);

	public static void Start(Action<LINQPadServerOpt>? optFun = null)
	{
		var opt = LINQPadServerOpt.Build(optFun);
		serD.Value = null;
		var d = serD.Value = new Disp();
		whenRefreshNeeded = new Subject<Unit>().D(d);
		Tweaks = new Tweaks();
		var imageFixer = new ImageFixer();
		var scriptTracker = new ScriptTracker();
		var pageBuilder = new PageBuilder(imageFixer, scriptTracker, Tweaks, opt.WsPort);
		var server = new Server(imageFixer, scriptTracker, pageBuilder, opt.HttpPort, opt.WsPort).D(d);
		WhenChg = server.WhenChg;

		var refreshRecPrev = May.None<RefreshRec>();
		var lastRefreshTime = DateTime.MinValue;
		var isRefreshing = false;

		var gate = new object();

		WhenRefreshNeeded
			.Throttle(PageServingConsts.RefreshDelay)
			.Synchronize(gate)
			.Subscribe(_ =>
			{
				var refreshRec = new RefreshRec(
					PageBuilder.GetHtmlStyle(),
					pageBuilder.GetTweakedHead(),
					pageBuilder.GetTweakedBody()
				);
				var refreshRecNext = May.Some(refreshRec);
				if (refreshRecNext == refreshRecPrev) return;

				isRefreshing = true;
				lastRefreshTime = DateTime.Now;
				refreshRecPrev = refreshRecNext;

				var str = refreshRec.Ser();
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

		var pageUrl = $"http://{PageBuilder.MachineName}:{opt.HttpPort}/";
		MakeConnectionUI(server.WSState, pageUrl, opt.HtmlEditFolder, server).Dump();
	}

	private static Control MakeConnectionUI(IRoVar<WSState> wsState, string pageUrl, string? htmlEditFolder, Server server)
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
			//new Hyperlink(pageUrl, pageUrl)
			new Label(pageUrl)
			{
				CssClass = "connection-link",
			},
			folderText = new TextBox(htmlEditFolder ?? string.Empty),
			new Button("Edit html", _ => server.EditHtml(folderText.Text))
		)
		{
			CssClass = "connection",
			HtmlElement =
			{
				ID = PageBuilder.ServerConnectionId
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