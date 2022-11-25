using LINQPad.Controls;
using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras.Scripting_Batcher;

static class BatcherConsts
{
	public static readonly string ConfirmationMessageColor = "#fae01b";
	public static readonly string OperationCancelledMessageColor = "#fa6969";


	public static C StyleLogLine<C>(this C dc) where C : Control =>
		dc
			.SetForeColor(ColLogPanel);

	private const string ColLogPanel = "#ffffff";



	public static C StyleTitle<C>(this C div) where C : Control =>
		div
			.Set("font-size", "24px")
			.Set("font-weight", "bold")
			.SetForeColor(ColTitle)
			.Set("margin", "10px 0");

	public static C StyleTitleHighlight<C>(this C div) where C : Control =>
		div
			.SetForeColor(ColTitleHighlight);

	private const string ColTitle = "#c0c0c0";

	private const string ColTitleHighlight = "#ffffff";
}