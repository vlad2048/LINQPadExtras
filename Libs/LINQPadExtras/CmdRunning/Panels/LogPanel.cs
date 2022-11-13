using LINQPad;
using LINQPadExtras.Styling;

namespace LINQPadExtras.CmdRunning.Panels;

class LogPanel
{
	public DumpContainer Root { get; }

	public LogPanel()
	{
		Root = new DumpContainer()
			.StyleLogPanel();
	}

	public void Log(string str) => Root.AppendContent(Html.Div(str));
}

static class LogPanelExt
{
	public static void LogNewline(this LogPanel logPanel) =>
		logPanel.Log(" ");

	public static void LogTitle(this LogPanel logPanel, string str)
	{
		logPanel.Log(str);
		logPanel.Log(new string('=', str.Length));
	}
}