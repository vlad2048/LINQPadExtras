using System.Diagnostics;
using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.DialogLogic;
using LINQPadExtras.DialogLogic.Enums;
using LINQPadExtras.Scripting_LockChecker.Utils;

namespace LINQPadExtras.Scripting_LockChecker;

public static class LockChecker
{
	private record ProcNfo(
		Process Proc,
		int Id,
		string Exe,
		string ExeFolder,
		string? Title
	)
	{
		public static ProcNfo Make(Process proc)
		{
			var exeFilename = proc.MainModule?.FileName;
			var (exe, exeFolder) = exeFilename switch
			{
				null => ("", ""),
				not null => (Path.GetFileName(exeFilename), Path.GetDirectoryName(exeFilename) ?? "")
			};
			return new ProcNfo(proc, proc.Id, exe, exeFolder, proc.MainWindowTitle);
		}
	}
	private record Lock(string Folder, ProcNfo Proc)
	{
		public bool Killed { get; set; }
		public Button KillBtn { get; set; }
	}

	public static bool CheckFolders(params string[] folders)
	{
		var locks = (
			from folder in folders
			where Directory.Exists(folder)
			from proc in LockFinder.WhoIsLockingFolder(folder)
			select new Lock(folder, ProcNfo.Make(proc))
		).ToArray();

		if (locks.Length == 0) return true;


		Button killAllBtn = null!;
		Button continueBtn = null!;

		void CheckAllKilled()
		{
			var allKilled = locks.All(e => e.Killed);
			killAllBtn.Enabled = !allKilled;
			continueBtn.Enabled = allKilled;
		}

		void KillLock(Lock lck)
		{
			if (lck.Killed) return;
			lck.Proc.Proc.Kill();
			lck.Killed = true;
			lck.KillBtn.Enabled = false;
			CheckAllKilled();
		}

		void KillAllLocks()
		{
			foreach (var lck in locks)
				KillLock(lck);
		}


		var hasCancelled = false;

		Dialoger.Run(
			"Folder lock detected",
			opt =>
			{
				opt.Type = DlgType.Error;
			},
			dlg =>
			{
				var cancelBtn = new Button("Cancel", _ =>
				{
					hasCancelled = true;
					dlg.Close();
				});
				dlg.AddButton(DlgBtnLocation.FooterLeft, DlgBtnType.Normal, cancelBtn);


				killAllBtn = new Button("Kill all", _ => KillAllLocks());
				dlg.AddButton(DlgBtnLocation.FooterRight, DlgBtnType.Normal, killAllBtn);

				continueBtn = new Button("Continue", _ =>
				{
					dlg.Close();
				})
				{
					Enabled = false
				};
				dlg.AddButton(DlgBtnLocation.FooterRight, DlgBtnType.Main, continueBtn);
			},
			dlg =>
			{
				dlg.DC.UpdateContent(
					locks
						.GroupBy(e => e.Folder)
						.Select(grp => Util.VerticalRun(
							MakeFolderCtrl(grp.Key),
							grp.Select(f => new
							{
								Id = f.Proc.Id,
								Exe = f.Proc.Exe, //.WithTooltip(f.Proc.ExeFolder),
								Action = f.KillBtn = new Button("Kill", _ =>
								{
									KillLock(f);
								})
								{
									IsMultithreaded = true
								}
							})
						))
				);
			}
		);

		return !hasCancelled;
	}

	private static Control MakeFolderCtrl(string folder) =>
		new("code")
		{
			HtmlElement =
			{
				InnerText = folder
			}
		};

	private static Div WithTooltip(this string str, string tooltip)
	{
		var div = new Div(new Span(str));
		var tooltipSpan = new Span(tooltip)
		{
			CssClass = "tooltiptext"
		};
		div.CssClass = "tooltip";
		div.Children.Add(tooltipSpan);
		return div;
	}
}