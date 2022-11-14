using LINQPad;
using PowRxVar;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LINQPad.Uncapsulator;

namespace LINQPadExtras;

/// <summary>
/// Utility class to make ProgressBars
/// </summary>
public static class ProgressBarMaker
{
	/// <summary>
	/// Makes a ProgressBar tied to a IRwVar&lt;double&gt; and returns that IRwVar:
	/// <list type="bullet">
	/// <item><description> The var is initially set to 1.0 </description></item>
	/// <item><description> When set to x=1.0 the progress bar is invisible </description></item>
	/// <item><description> When set 0.0&lt;=x&lt;1.0 the progress bar is visible </description></item>
	/// </list>
	/// </summary>
	/// <param name="title">Title</param>
	/// <returns>The IRwVar that represents completion between 0.0 and 1.0</returns>
	public static (IRwVar<double>, IDisposable) Make(string title)
	{
		var d = new Disp();
		var val = Var.Make(1.0).D(d);

		var progressBar = new Util.ProgressBar(title).Dump();
		var dc = progressBar.Uncapsulate()._dumpContainer;
		val
			.ObserveOn(NewThreadScheduler.Default)
			.Subscribe(v =>
			{
				var isEnd = Math.Abs(1.0 - v) < 0.001;
				dc.Style = isEnd switch
				{
					true => "display:none",
					false => string.Empty
				};
				progressBar.Fraction = v;
			}).D(d);

		return (val, d);
	}
}