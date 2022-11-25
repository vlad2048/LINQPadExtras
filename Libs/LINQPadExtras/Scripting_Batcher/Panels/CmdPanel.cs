using System.Reactive.Linq;
using LINQPad.Controls;
using LINQPad;
using PowRxVar;
using System.Reactive.Subjects;
using LINQPadExtras.Scripting_Batcher.Panels.Ctrls;
using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras.Scripting_Batcher.Panels;

enum CmdPanelMode
{
	Normal,
	LeaveOpen,
	LogCmdOnly
}

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

	public CmdPanel(string exeFile, string args, CmdPanelMode mode)
	{
		StatusBtn statusBtn;
		Div headerDiv;
		Div outputDiv;
		DumpContainer outputDC;

		Root =
			new Div(
					headerDiv = new Div(
							(statusBtn = new StatusBtn(mode == CmdPanelMode.LogCmdOnly).D(d)).Root,
							new Span(exeFile).WithClass("cmdpanel-header-exe"),
							new Span(args).WithClass("cmdpanel-header-args")
						).MultiThread()
						.WithClass("cmdpanel-header"),

					outputDiv = new Div(outputDC = new DumpContainer()).WithClass("cmdpanel-output")
				)
				.WithClass("cmdpanel");

		WhenComplete
			.Delay(TimeSpan.FromSeconds(1))
			.Subscribe(success =>
			{
				headerDiv.CssClass = "cmdpanel-header cmdpanel-header-active";
				statusBtn.State.V = (success && mode == CmdPanelMode.Normal) switch
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
					sectionDC = new DumpContainer();
					outputDC.AppendContent(new Div(sectionDC).WithClass(msg.Std == Std.Err ? "cmdpanel-output-stderr" : "cmdpanel-output-stdout"));
				}
				var lineDiv = new Div(new Span(msg.Str));
				sectionDC.AppendContent(lineDiv);
			}).D(d);

		statusBtn.State
			.Subscribe(state =>
			{
				outputDiv.Set("display", state is StatusState.None or StatusState.Closed ? "none" : "block");
			}).D(d);
	}
}