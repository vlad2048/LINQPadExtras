using System.Reactive;
using System.Reactive.Linq;
using LINQPad.Controls;
using PowRxVar;

namespace LINQPadExtras.Utils;

static class EventExt
{
	public static IObservable<Unit> WhenClick(this Control ctrl) =>
		Observable.FromEventPattern(e => ctrl.Click += e, e => ctrl.Click -= e).ToUnit();
}