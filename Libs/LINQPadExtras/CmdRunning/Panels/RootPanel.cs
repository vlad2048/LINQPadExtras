using LINQPad;
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

	public static void Clear() => DC.ClearContent();

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
}