using CliWrap;
using LINQPad;
using LINQPadExtras.CmdRunning;
using LINQPadExtras.CmdRunning.Panels;
using LINQPadExtras.Utils;
using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras;

/// <summary>
/// Console
/// </summary>
public static class Con
{
	private static CmdState? state;
	private static CmdState State
	{
		get
		{
			RestartDetector.OnRestart(nameof(Con), OnRestart);
			return state!;
		}
	}
	private static void OnRestart()
	{
		state = new CmdState();
	}


	/// <summary>
	/// Console UI
	/// </summary>
	public static DumpContainer Root => RootPanel.Root;

	/// <summary>
	/// Clear the console and display a new title
	/// </summary>
	public static void Start(string title)
	{
		RootPanel.Clear();
		State.Clear();
		RootPanel.MakeTitle(title);
	}

	/// <summary>
	/// Clear the console and display a new title in 3 parts
	/// </summary>
	public static void Start(string prefix, string title, string suffix)
	{
		RootPanel.Clear();
		State.Clear();
		RootPanel.MakeTitleSandwich(prefix, title, suffix);
	}

	/// <summary>
	/// Add an artifact to display at the end
	/// </summary>
	public static void AddArtifact(string artifact) => State.AddArtifact(artifact);

	/// <summary>
	/// Display the end title
	/// </summary>
	public static void EndSuccess()
	{
		RootPanel.MakeTitle("Success");
		var logPanel = RootPanel.MakeLogPanel();
		logPanel.Log($"{State.Artifacts.Count} artifacts:");
		for (var i = 0; i < State.Artifacts.Count; i++)
			logPanel.LogArtifact(i, State.Artifacts[i]);
	}

	/// <summary>
	/// Log a message to the console
	/// </summary>
	/// <param name="msg">message</param>
	public static void Log(string msg)
	{
		var logPanel = RootPanel.MakeLogPanel();
		logPanel.Log(msg);
	}


	/// <summary>
	/// Run a command in the console
	/// </summary>
	/// <param name="exeFile">exe to run</param>
	/// <param name="args">arguments</param>
	/// <returns>output</returns>
	public static string Run(string exeFile, params string[] args) => RunIn(exeFile, Path.GetDirectoryName(exeFile)!, args);
	
	/// <summary>
	/// Run a command in the console, leave the output open after it finishes
	/// </summary>
	/// <param name="exeFile">exe to run</param>
	/// <param name="workingDirectory">working directory</param>
	/// <param name="args">arguments</param>
	/// <returns>output</returns>
	public static string RunInLeaveOpen(string exeFile, string workingDirectory, params string[] args) =>
		RunIn(exeFile, workingDirectory, true, args);

	/// <summary>
	/// Run a command in the console
	/// </summary>
	/// <param name="exeFile">exe to run</param>
	/// <param name="workingDirectory">working directory</param>
	/// <param name="args">arguments</param>
	/// <returns>output</returns>
	public static string RunIn(string exeFile, string workingDirectory, params string[] args) =>
		RunIn(exeFile, workingDirectory, false, args);

	private static string RunIn(string exeFile, string workingDirectory, bool leaveOpenAfter, params string[] args)
	{
		var cmd = Cli.Wrap(exeFile)
			.WithWorkingDirectory(workingDirectory)
			.WithArguments(args)
			.WithValidation(CommandResultValidation.None);
		var niceExeFile = State.ChangeDir(cmd.TargetFilePath, cmd.WorkingDirPath);
		return cmd
			.Run(niceExeFile, leaveOpenAfter);
	}

	/// <summary>
	/// Delete a file
	/// </summary>
	/// <param name="file">file</param>
	public static void DeleteFile(string file)
	{
		if (!File.Exists(file)) return;
		ShowCmd("del", $"/q {file.QuoteIFN()}");
		ProcessKiller.RunWithKillProcessRetry(
			() => File.Delete(file),
			$"delete file: '{file}'",
			file,
			false
		);
	}

	/// <summary>
	/// Delete a folder
	/// </summary>
	/// <param name="folder">folder</param>
	public static void DeleteFolder(string folder)
	{
		if (!Directory.Exists(folder)) return;
		ShowCmd("rmdir", $"/s /q {folder.QuoteIFN()}");
		ProcessKiller.RunWithKillProcessRetry(
			() => Directory.Delete(folder, true),
			$"delete folder: '{folder}'",
			folder,
			true
		);
	}

	/// <summary>
	/// Create a folder
	/// </summary>
	/// <param name="folder">folder</param>
	public static void MakeFolder(string folder)
	{
		if (Directory.Exists(folder)) return;
		ShowCmd("mkdir", folder.QuoteIFN());
		Directory.CreateDirectory(folder);
	}

	/// <summary>
	/// Empty a folder
	/// </summary>
	/// <param name="folder">folder</param>
	public static void EmptyFolder(string folder)
	{
		if (Directory.Exists(folder))
		{
			if (Directory.GetFileSystemEntries(folder).Length == 0) return;
			DeleteFolder(folder);
		}

	}

	internal static void ShowCmd(string exeFile, string args) => RootPanel.MakeCmdPanel(exeFile, args, true, false);
}