using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.Styling;
using LINQPadExtras.Utils;

namespace LINQPadExtras.CmdRunning.Panels;

static class RootPanel
{
	private static DumpContainer? dc;
	private static DumpContainer DC => dc!;

	public static DumpContainer Root
	{
		get
		{
			if (dc != null) return dc;
			RestartDetector.OnRestart(nameof(RootPanel), OnRestartNoDump);
			return DC;
		}
	}

	private static void OnRestartNoDump()
	{
		dc = new DumpContainer()
			.StyleRootPanel();
	}

	private static void OnRestart()
	{
		dc = new DumpContainer()
			.StyleRootPanel()
			.Dump();
	}

	public static void Clear()
	{
		RestartDetector.OnRestart(nameof(RootPanel), OnRestart);
		DC.ClearContent();
	}

	public static CmdPanel MakeCmdPanel(string exeFile, string args, bool showCmdOnly, bool leaveOpenAfter)
	{
		RestartDetector.OnRestart(nameof(RootPanel), OnRestart);
		var cmdPanel = new CmdPanel(exeFile, args, showCmdOnly, leaveOpenAfter);
		DC.AppendContent(cmdPanel.Root);
		return cmdPanel;
	}

	public static LogPanel MakeLogPanel()
	{
		RestartDetector.OnRestart(nameof(RootPanel), OnRestart);
		var logPanel = new LogPanel();
		DC.AppendContent(logPanel.Root);
		return logPanel;
	}

	public static void MakeTitleSandwich(string prefix, string title, string suffix)
	{
		RestartDetector.OnRestart(nameof(RootPanel), OnRestart);
		DC.AppendContent(Util.HorizontalRun(true,
			new Span(prefix).StyleTitle(),
			new Span(title).StyleTitle().StyleTitleHighlight(),
			new Span(suffix).StyleTitle()
		));
	}

	public static void MakeTitle(string title)
	{
		RestartDetector.OnRestart(nameof(RootPanel), OnRestart);
		DC.AppendContent(
			Html.Div(title)
				.StyleTitle()
		);
	}
}