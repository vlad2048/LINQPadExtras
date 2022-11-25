using LINQPad;
using PowRxVar;
using System.Reactive.Linq;
using LINQPad.Controls;
using LINQPadExtras.Utils.Exts;
using PowBasics.CollectionsExt;

namespace LINQPadExtras.Scripting_Batcher.Panels.Ctrls;

enum StatusState
{
	None,
	Running,
	Open,
	Closed,
}

class StatusBtn : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	public IRwVar<StatusState> State { get; }

	public Div Root { get; }

	public StatusBtn(bool isNone)
	{
		State = Var.Make(isNone ? StatusState.None : StatusState.Running).D(d);
		//Root = new DumpContainer().SetForeColor(StatusBtnConsts.StatusCol);

		var dc = new DumpContainer();
		Root = new Div(dc).WithClass("cmdpanel-header-statusbtn");

		State
			.Select(_state => _state switch
			{
				StatusState.None =>
					Observable.Return(StatusBtnConsts.StatusNoneChar),

				StatusState.Running =>
					Observable.Interval(StatusBtnConsts.StatusSpinnerInterval).Zip(StatusBtnConsts.StatusSpinnerChars.RepeatInfinitely()).Select(t => t.Second),

				StatusState.Open =>
					Observable.Return(StatusBtnConsts.StatusOpenChar),

				StatusState.Closed =>
					Observable.Return(StatusBtnConsts.StatusClosedChar),

				_ => throw new ArgumentException()
			})
			.Switch()
			.Subscribe(ch =>
			{
				dc.UpdateContent(ch);
			}).D(d);
	}
}