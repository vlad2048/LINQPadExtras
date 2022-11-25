using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.DialogLogic.Enums;
using LINQPadExtras.Utils.Exts;
using PowRxVar;

namespace LINQPadExtras.DialogLogic;

public static class Dialoger
{
	public static void Run(
		string title,
		Action<DialogerOpt>? optFun,
		Action<IDlg> setupFun,
		Action<IDlg> runFun
	)
	{
		ModuleInit.Check();
		var opt = DialogerOpt.Make(optFun);
		var headerTitle = new Span(title)
		{
			Styles =
			{
				["color"] = opt.Type switch
				{
					DlgType.Normal => "var(--modal-type-normal-color)",
					DlgType.Error => "var(--modal-type-error-color)",
					_ => throw new ArgumentException()
				}
			}
		};
		/*var headerCloseIcon = new Svg(
			"""
			<line x1="18" y1="6" x2="6" y2="18"/>
			<line x1="6" y1="6" x2="18" y2="18"/>
			<line x1="18" y1="6" x2="6" y2="18"/>
			<line x1="6" y1="6" x2="18" y2="18"/>
			""", 24, 24, "0 0 24 24"
		);*/
		var header = new Div(headerTitle/*, headerCloseIcon*/) { CssClass = "modal-header" };

		var dc = new DumpContainer();
		var mainInner = new Div(dc) { CssClass = "modal-main-inner" }.MultiThread();
		var main = new Div(mainInner) { CssClass = "modal-main" };

		var footerLeft = new Div { CssClass = "modal-footer-part" };
		var footerRight = new Div { CssClass = "modal-footer-part" };
		var footer = new Div(footerLeft, footerRight) { CssClass = "modal-footer" };

		var inner = new Div(header, main, footer)
		{
			CssClass = opt.Maximize switch
			{
				false => "modal-inner",
				true => "modal-inner modal-inner-max"
			}
		};
		var root = new Control("dialog", inner) { CssClass = "modal" };

		using var dlg = new Dlg(dc, root, inner, mainInner, footerLeft, footerRight);

		setupFun(dlg);
		root.Dump();
		try
		{
			runFun(dlg);
		}
		catch (Exception ex)
		{
			if (ex is AggregateException or OperationCanceledException)
			{
				"Cancelled".Dump();
			}
			else
			{
				throw;
			}
		}

		using var slim = new ManualResetEventSlim();
		dlg.WhenClosed.Subscribe(_ => slim.Set()).D(dlg.D);

		slim.Wait();
	}
}