using System.Diagnostics;
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
		var loc = new Loc(path, isFolder, actionName);

		var sorted = HandleLock(loc, null);
		if (!sorted)
			throw new ArgumentException($"Failed {loc.ActionName} @ '{loc.Path}' (isFolder:{loc.IsFolder})");

		retry:
		try
		{
			action();
		}
		catch (Exception ex)
		{
			var retry = HandleLock(loc, ex);
			if (retry)
				goto retry;
			throw;
		}
	}

	private record Loc(string Path, bool IsFolder, string ActionName)
	{
		public List<Process> GetProcessesLockingIt() => IsFolder switch
		{
			false => LockFinder.WhoIsLockingFile(Path),
			true => LockFinder.WhoIsLockingFolder(Path),
		};
	}


	private static bool HandleLock(
		Loc loc,
		Exception? ex
	)
	{
		if (ex is not null && ex is not IOException and not UnauthorizedAccessException)
			throw ex;

		var logPanel = new Lazy<LogPanel>(RootPanel.MakeLogPanel);
		LogPanel LogPanel() => logPanel.Value;

		try
		{
			var procs = loc.GetProcessesLockingIt();
			if (ex == null && procs.Count == 0)
				return true;

			LogPanel().LogNewline();
			if (ex == null)
			{
				LogPanel().Log($"Detected a lock before {loc.ActionName}");
			}
			else
			{
				LogPanel().Log($"Exception while {loc.ActionName}");
				LogPanel().Log(ex.Message);
			}

			LogPanel().LogNewline();
			LogPanel().LogTitle("Processes holding the lock");
			var thisProcId = Environment.ProcessId;
			foreach (var proc in procs)
			{
				var procId = proc.Id;
				var procName = proc.ProcessName;
				var procModuleName = proc.MainModule?.FileName;
				var procWinTitle = proc.MainWindowTitle;
				LogPanel().Log($"  {procName}");
				LogPanel().Log($"  {new string('-', procName.Length)}");
				LogPanel().Log($"    Id      : {procId}");
				LogPanel().Log($"    Module  : {procModuleName}");
				LogPanel().Log($"    WinTitle: {procWinTitle}");
				if (procId == thisProcId)
					LogPanel().Log("    !! This is this Process -> cannot kill myself");

				LogPanel().LogNewline();
			}

			var msg = procs.Any() ? "Kill and retry (y/n) ?" : "Retry (y/n) ?";
			LogPanel().LogNewline();
			LogPanel().Log(msg);
			if (Util.ReadLine(msg).ToLowerInvariant().Trim() == "y")
			{
				procs.ForEach(proc =>
				{
					if (proc.Id == thisProcId)
					{
						LogPanel().Log($"  skipping {proc.ProcessName}");
						return;
					}

					LogPanel().Log($"  killing {proc.ProcessName}");
					proc.Kill();
					LogPanel().Log($"  killed {proc.ProcessName}");
				});
				Thread.Sleep(TimeSpan.FromMilliseconds(200));
				return true;
			}

			return false;
		}
		catch (Exception exInner)
		{
			LogPanel().Log($"ExceptionInner: {exInner.Message}");
			throw;
		}
	}
}