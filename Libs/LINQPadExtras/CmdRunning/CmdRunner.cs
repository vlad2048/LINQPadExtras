using System.Text;
using CliWrap;
using CliWrap.Exceptions;
using LINQPadExtras.CmdRunning.Panels;
using LINQPadExtras.Utils;
using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras.CmdRunning;

static class CmdRunner
{
	private static string? curFolder;

	private static void OnRestart()
	{
		curFolder = null;
	}

	public static string Run(this Command cmd, bool leaveOpenAfter)
	{
		RestartDetector.OnRestart(nameof(CmdRunner), OnRestart);

		var exeFile = ShowDirChangeAndSimplifyExe(cmd.TargetFilePath, cmd.WorkingDirPath);
		var args = cmd.Arguments;
		var cmdPanel = RootPanel.MakeCmdPanel(exeFile, args, false, leaveOpenAfter);

		try
		{
			var sbOut = new StringBuilder();

			cmd
				.WithStandardOutputPipe(
					PipeTarget.Merge(
						PipeTarget.ToDelegate(cmdPanel.StdOut),
						PipeTarget.ToStringBuilder(sbOut)
					)
				)
				.WithStandardErrorPipe(PipeTarget.ToDelegate(cmdPanel.StdErr))
				.WithValidation(CommandResultValidation.ZeroExitCode)
				.ExecuteAsync()
				.GetAwaiter()
				.GetResult();

			cmdPanel.Complete(true);

			return sbOut.ToString();
		}
		catch (CommandExecutionException)
		{
			cmdPanel.Complete(false);
			throw;
		}
	}

	private static string ShowDirChangeAndSimplifyExe(string exeFile, string workingDirectory)
	{
		if (workingDirectory != curFolder)
		{
			curFolder = workingDirectory;
			Con.ShowCmd("cd", $"/d {curFolder.QuoteIFN()}");
		}

		return (Path.GetDirectoryName(exeFile) == workingDirectory) switch
		{
			true => Path.GetFileName(exeFile),
			false => exeFile
		};
	}
}