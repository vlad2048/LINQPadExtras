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
	/// <summary>
	/// Console UI
	/// </summary>
	public static DumpContainer Root => RootPanel.Root;

	/// <summary>
	/// Clear the console
	/// </summary>
	public static void Clear() => RootPanel.Clear();

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
		return Cli.Wrap(exeFile)
			.WithWorkingDirectory(workingDirectory)
			.WithArguments(args)
			.WithValidation(CommandResultValidation.None)
			.Run(leaveOpenAfter);
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