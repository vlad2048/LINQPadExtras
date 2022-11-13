using System.Reactive.Linq;
using System.Reactive.Subjects;
using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.Styling;
using LINQPadExtras.Utils;
using PowRxVar;

namespace LINQPadExtras.CmdRunning.Panels;

class CmdPanel : IDisposable
{
	private enum Std
	{
		Out,
		Err
	}
	private record Msg(Std Std, string Str);

	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly ISubject<Msg> whenStd = new Subject<Msg>();
	private readonly ISubject<bool> whenComplete = new Subject<bool>();
	private IObservable<Msg> WhenStd => whenStd.AsObservable();
	private IObservable<bool> WhenComplete => whenComplete.AsObservable();

	public Div Root { get; }
	public void StdOut(string str) => whenStd.OnNext(new Msg(Std.Out, str));
	public void StdErr(string str) => whenStd.OnNext(new Msg(Std.Err, str));
	public void Complete(bool success) => whenComplete.OnNext(success);

	public CmdPanel(string exeFile, string args, bool showCmdOnly, bool leaveOpenAfter)
	{
		StatusBtn statusBtn;
		Div headerDiv;
		DumpContainer outputDC;

		Root =
			new Div(
					headerDiv = new Div(
							(statusBtn = new StatusBtn(showCmdOnly).D(d)).Root,
							new Span(exeFile).StyleTitleExePanel(),
							new Span(args).StyleTitleArgsPanel()
						)
						.StyleHeaderPanel(),

					outputDC = new DumpContainer()
						.StyleOutputPanel()
				)
				.StyleCmdPanel();

		WhenComplete.Subscribe(success =>
		{
			statusBtn.State.V = (success && !leaveOpenAfter) switch
			{
				true => StatusState.Closed,
				false => StatusState.Open
			};
		}).D(d);

		headerDiv.WhenClick()
			.Where(_ => statusBtn.State.V is not StatusState.None and not StatusState.Running)
			.Subscribe(_ => statusBtn.State.V = statusBtn.State.V == StatusState.Closed ? StatusState.Open : StatusState.Closed).D(d);

		DumpContainer? sectionDC = null;
		var sectionStd = Std.Out;

		WhenStd
			.Subscribe(msg =>
			{
				if (sectionDC == null || msg.Std != sectionStd)
				{
					sectionStd = msg.Std;
					sectionDC = new DumpContainer().StyleSectionPanel(msg.Std == Std.Err);
					outputDC.AppendContent(sectionDC);
				}
				var lineDiv = Html.Div(msg.Str);
				sectionDC.AppendContent(lineDiv);
			}).D(d);

		statusBtn.State
			.Subscribe(state =>
			{
				outputDC.Set("display", state is StatusState.None or StatusState.Closed ? "none" : "block");
			}).D(d);
	}
}