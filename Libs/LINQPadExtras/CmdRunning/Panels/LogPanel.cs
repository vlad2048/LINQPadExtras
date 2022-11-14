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

	public void LogArtifact(int idx, string artifact) => Root.AppendContent(Util.HorizontalRun(true,
		Html.Div($"  [{idx}]: "),
		new Hyperlinq(artifact.GetArtifactLink(), artifact)
	));
}

static class LogPanelExt
{
	public static string GetArtifactLink(this string artifact) => (Directory.Exists(artifact), File.Exists(artifact)) switch
	{
		(true, false) => artifact,
		(false, true) => Path.GetDirectoryName(artifact) ?? artifact,
		_ => artifact
	};

	public static void LogNewline(this LogPanel logPanel) =>
		logPanel.Log(" ");

	public static void LogTitle(this LogPanel logPanel, string str)
	{
		logPanel.Log(str);
		logPanel.Log(new string('=', str.Length));
	}
}