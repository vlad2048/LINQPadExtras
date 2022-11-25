using LINQPad.Controls;

namespace LINQPadExtras.Utils.Exts;

public static class ControlFluentExt
{
	public static C WithClass<C>(this C ctrl, string cssClass) where C : Control
	{
		ctrl.CssClass = cssClass;
		return ctrl;
	}

	public static C MultiThread<C>(this C ctrl) where C : Control
	{
		ctrl.IsMultithreaded = true;
		return ctrl;
	}
}