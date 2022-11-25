using LINQPad.Controls;
using LINQPadExtras.DialogLogic;
using LINQPadExtras.DialogLogic.Enums;
using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras.Scripting_Batcher;


public static class Batcher
{
	public static void Run(bool dryRun, string title, Action<ICmd> cmdFun)
	{
		Button cancelBtn = null!;
		Button closeBtn = null!;

		using var cmd = new Cmd(dryRun);

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
				cmd.DC = dlg.DC;

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
						dlg.DC.AppendContent("\n");
						var msgDiv = new Div(new Span("Operation cancelled")).SetForeColor(BatcherConsts.OperationCancelledMessageColor).Set("font-weight", "bold");
						dlg.DC.AppendContent(msgDiv);
						dlg.DC.AppendContent("\n");

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