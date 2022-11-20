using System.Diagnostics;
using System.Text;
using LINQPad;
using LINQPadExtras.CmdRunning.Panels;

namespace LINQPadExtras.Utils;

static class ProcessKiller
{
	public static bool CheckForFoldersLock(params string[] folders) =>
		folders
			.Where(Directory.Exists)
			.Any(CheckForFolderLock);
	
	private static bool CheckForFolderLock(string folder)
	{
		Process[] GetLockProcs() => LockFinder.WhoIsLockingFolder(folder).ToArray();

		var isLocked = true;
		while (isLocked)
		{
			var procs = GetLockProcs();
			isLocked = procs.Any();
			if (isLocked)
			{
				var l = RootPanel.MakeLogPanel();
				var cancel = l.AskUserToKillProcesses(folder, procs);
				if (cancel)
					return true;
				l.KillProcs(procs);
			}
		}
		return false;
	}

	private static bool AskUserToKillProcesses(this LogPanel l, string folder, Process[] procs)
	{
		l.LogNewline();
		l.LogTitle($"Lock detected on '{folder}'");
		l.LogProcs(procs);
		return l.AskUserOrExit();
	}

	private static void LogProcs(this LogPanel l, Process[] procs)
	{
		string Fmt(string? s) => s switch
		{
			not null => s,
			null => "_"
		};
		var thisProcId = Environment.ProcessId;
		for (var i = 0; i < procs.Length; i++)
		{
			var proc = procs[i];
			var myselfStr = (proc.Id == thisProcId) switch
			{
				true => " (this is this process itself !)",
				false => string.Empty
			};
			l.Log($"  [{i}] Process {proc.ProcessName}{myselfStr}");
			l.Log($"    id    : {proc.Id}");
			l.Log($"    module: {Fmt(proc.MainModule?.FileName)}");
			l.Log($"    title : {proc.MainWindowTitle}");
		}
	}

	private static void KillProcs(this LogPanel l, Process[] procs)
	{
		var thisProcId = Environment.ProcessId;
		for (var i = 0; i < procs.Length; i++)
		{
			var proc = procs[i];
			var sb = new StringBuilder($"[{i + 1}/{procs.Length}] killing {proc.Id}");
			if (proc.Id == thisProcId)
				sb.Append(" (this is myself!)");
			try
			{
				proc.Kill(true);
				sb.AppendLine(" -> done");
			}
			catch (Exception ex)
			{
				sb.AppendLine($" -> exception:{ex.Message}");
			}
			l.Log(sb.ToString());
		}
	}

	private static bool AskUserOrExit(this LogPanel l)
	{
		var msg = "Kill and retry (y/n) ?";
		l.LogNewline();
		l.Log(msg);
		return Util.ReadLine(msg).ToLowerInvariant().Trim() != "y";
	}



	public static void RunWithKillProcessRetry(
		Action action,
		string actionName,
		string path,
		bool isFolder
	)
	{
		var loc = new Loc(path, isFolder, actionName);
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