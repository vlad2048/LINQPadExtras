using LINQPad.Controls;
using LINQPad.FSharpExtensions;
using LINQPadExtras.DialogLogic;
using LINQPadExtras.DialogLogic.Enums;
using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras.Scripting_Batcher;

public class BatcherOpt
{
	public bool DryRun { get; set; }
	public bool CmdLine { get; set; }

	internal static BatcherOpt Build(Action<BatcherOpt>? optFun)
	{
		var opt = new BatcherOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

public static class Batcher
{
	public static void Run(string title, Action<ICmd> cmdFun, Action<BatcherOpt>? optFun = null)
	{
		var opt = BatcherOpt.Build(optFun);
		Button cancelBtn = null!;
		Button closeBtn = null!;

		var cmdDisp = opt.CmdLine switch
		{
			true => (ICmdDisp)new ConCmdDisp(),
			false => new GuiCmdDisp()
		};
		using var cmd = new Cmd(cmdDisp, opt.DryRun);


		if (opt.CmdLine)
		{
			try
			{
				cmdFun(cmd);
				cmd.LogArtifacts();
			}
			catch (OperationCanceledException)
			{
			}
			if (cmd.IsCancelled)
			{
				if (!cmd.LeaveOpenAfter)
				{
					cmdDisp.LogError("Operation cancelled");

					//Thread.Sleep(1000);
					//dlg.Close();
				}
			}

			return;
		}


		Dialoger.Run(
			title,
			opt =>
			{
				opt.Maximize = true;
			},
			dlg =>
			{
				dlg.DCWrap.Set("font-family", "consolas");
				cancelBtn = new Button("Cancel", _ =>
				{
					cmd.Cancel();
				});
				dlg.AddButton(DlgBtnLocation.FooterLeft, DlgBtnType.Normal, cancelBtn);

				var copyToClipboardBtn = new Button("Copy commands", _ =>
				{
					cmd.CopyToClipboard();
				});
				dlg.AddButton(DlgBtnLocation.FooterRight, DlgBtnType.Normal, copyToClipboardBtn);

				closeBtn = new Button("Close", _ => dlg.Close())
				{
					Enabled = false
				};
				dlg.AddButton(DlgBtnLocation.FooterRight, DlgBtnType.Main, closeBtn);
			},
			dlg =>
			{
				cmd.HookDC(dlg.DC);

				try
				{
					cmdFun(cmd);
					cmd.LogArtifacts();
				}
				catch (OperationCanceledException)
				{
				}
				if (cmd.IsCancelled)
				{
					if (!cmd.LeaveOpenAfter)
					{
						cmdDisp.LogError("Operation cancelled");

						Thread.Sleep(1000);
						dlg.Close();
					}
				}

				closeBtn.Enabled = true;
				cancelBtn.Enabled = false;
			}
		);
	}
}