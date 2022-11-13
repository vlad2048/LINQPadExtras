using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.Styling;

namespace LINQPadExtras.CmdRunning;

static class CmdConsts
{
	public static DumpContainer StyleRootPanel(this DumpContainer dc) =>
		dc
			.SetMono()
			.Set("margin", "5px");

	public static C StyleCmdPanel<C>(this C div) where C : Control =>
		div;

	public static C StyleHeaderPanel<C>(this C div) where C : Control =>
		div
			.Set("display", "flex")
			.Set("column-gap", "5px")
			.Set("cursor", "pointer");

	public static C StyleTitleExePanel<C>(this C div) where C : Control =>
		div
			.SetForeColor(ColTitleExe);

	public static C StyleTitleArgsPanel<C>(this C div) where C : Control =>
		div
			.SetForeColor(ColTitleArgs);

	public static DumpContainer StyleOutputPanel(this DumpContainer dc) =>
		dc;

	public static DumpContainer StyleSectionPanel(this DumpContainer dc, bool isErr) =>
		isErr switch
		{
			false => dc,
			true => dc
				.SetBackColor(ColErrSection)
		};

	public static C StyleLinePanel<C>(this C div) where C : Control =>
		div;

	

	public static DumpContainer StyleLogPanel(this DumpContainer dc) =>
		dc
			.SetForeColor(ColLogPanel);

	public static readonly TimeSpan StatusSpinnerInterval = TimeSpan.FromMilliseconds(150);
	public const string StatusCol = "#cbd769";
	public static readonly char[] StatusSpinnerChars =
	{
		'⠟',
		'⠯',
		'⠷',
		'⠾',
		'⠽',
		'⠻'
	};
	public const char StatusNoneChar = ' ';
	public const char StatusOpenChar = '▼';
	public const char StatusClosedChar = '▲';


	private static DumpContainer SetMono(this DumpContainer dc) =>
		dc
			.Set("font-family", "consolas")
			.SetColors("#32FEC4", "#030526");

	private const string ColTitleExe = "#1acd59";

	private const string ColTitleArgs = "#1fcbb4";

	private const string ColErrSection = "#61221d";

	private const string ColLogPanel = "#ffffff";
}