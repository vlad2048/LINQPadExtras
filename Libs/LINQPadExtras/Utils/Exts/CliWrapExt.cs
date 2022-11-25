using CliWrap;

namespace LINQPadExtras.Utils.Exts;

static class CliWrapExt
{
	public static Command WithWorkingDirectoryOpt(this Command cmd, string? folder) => folder switch
	{
		null => cmd,
		not null => cmd.WithWorkingDirectory(folder)
	};
}