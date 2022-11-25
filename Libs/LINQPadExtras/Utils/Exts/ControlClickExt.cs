using System.Reactive.Linq;
using System.Reactive;
using LINQPad.Controls;
using PowRxVar;

namespace LINQPadExtras.Utils.Exts;

static class ControlClickExt
{
	public static IObservable<Unit> WhenClick(this Control ctrl) =>
		Observable.FromEventPattern(e => ctrl.Click += e, e => ctrl.Click -= e).ToUnit();
}