using System.Text;
using CliWrap;
using CliWrap.Exceptions;
using LINQPadExtras.CmdRunning.Panels;

namespace LINQPadExtras.CmdRunning;

static class CmdRunner
{
	public static string Run(this Command cmd, string niceExeFile, bool leaveOpenAfter)
	{
		var args = cmd.Arguments;
		var cmdPanel = RootPanel.MakeCmdPanel(niceExeFile, args, false, leaveOpenAfter);

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
}