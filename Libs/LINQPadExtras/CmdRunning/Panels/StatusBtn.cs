using System.Reactive.Linq;
using LINQPad;
using LINQPadExtras.Styling;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace LINQPadExtras.CmdRunning.Panels;


public enum StatusState
{
	None,
	Running,
	Open,
	Closed,
}

public class StatusBtn : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	public IRwVar<StatusState> State { get; }

	public DumpContainer Root { get; }

	public StatusBtn(bool isNone)
	{
		State = Var.Make(isNone ? StatusState.None : StatusState.Running).D(d);
		Root = new DumpContainer()
			.SetForeColor(CmdConsts.StatusCol);

		State
			.Select(_state => _state switch
			{
				StatusState.None =>
					Observable.Return(CmdConsts.StatusNoneChar),

				StatusState.Running =>
					Observable.Interval(CmdConsts.StatusSpinnerInterval).Zip(CmdConsts.StatusSpinnerChars.RepeatInfinitely()).Select(t => t.Second),

				StatusState.Open =>
					Observable.Return(CmdConsts.StatusOpenChar),

				StatusState.Closed =>
					Observable.Return(CmdConsts.StatusClosedChar),

				_ => throw new ArgumentException()
			})
			.Switch()
			.Subscribe(ch =>
			{
				Root.UpdateContent(ch);
			}).D(d);
	}
}