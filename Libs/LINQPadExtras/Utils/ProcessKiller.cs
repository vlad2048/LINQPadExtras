using LINQPad;
using LINQPadExtras.CmdRunning.Panels;

namespace LINQPadExtras.Utils;

static class ProcessKiller
{
	public static void RunWithKillProcessRetry(
		Action action,
		string actionName,
		string path,
		bool isFolder
	)
	{
	retry:
		try
		{
			action();
		}
		catch (SystemException ex)
		{
			if (ex is not IOException and not UnauthorizedAccessException)
				throw;

			try
			{
				var logPanel = RootPanel.MakeLogPanel();

				logPanel.LogNewline();
				logPanel.Log($"Failed to {actionName}");
				logPanel.Log(ex.Message);
				logPanel.LogNewline();
				var procs = isFolder switch
				{
					false => LockFinder.WhoIsLockingFile(path),
					true => LockFinder.WhoIsLockingFolder(path),
				};
				logPanel.LogTitle("Processes holding the lock");
				var thisProcId = Environment.ProcessId;
				foreach (var proc in procs)
				{
					var procId = proc.Id;
					var procName = proc.ProcessName;
					var procModuleName = proc.MainModule?.FileName;
					var procWinTitle = proc.MainWindowTitle;
					logPanel.Log($"  {procName}");
					logPanel.Log($"  {new string('-', procName.Length)}");
					logPanel.Log($"    Id      : {procId}");
					logPanel.Log($"    Module  : {procModuleName}");
					logPanel.Log($"    WinTitle: {procWinTitle}");
					if (procId == thisProcId)
						logPanel.Log("    !! This is this Process -> cannot kill myself");

					logPanel.LogNewline();
				}

				var msg = procs.Any() ? "Kill and retry (y/n) ?" : "Retry (y/n) ?";
				logPanel.LogNewline();
				logPanel.Log(msg);
				if (Util.ReadLine(msg).ToLowerInvariant().Trim() == "y")
				{
					procs.ForEach(proc =>
					{
						if (proc.Id == thisProcId)
						{
							logPanel.Log($"  skipping {proc.ProcessName}");
							return;
						}

						logPanel.Log($"  killing {proc.ProcessName}");
						proc.Kill();
						logPanel.Log($"  killed {proc.ProcessName}");
					});
					Thread.Sleep(TimeSpan.FromMilliseconds(200));
					goto retry;
				}

				throw;
			}
			catch (Exception ex2)
			{
				$"Exception in catch: {ex2}".Dump();
				throw;
			}
		}
	}
}