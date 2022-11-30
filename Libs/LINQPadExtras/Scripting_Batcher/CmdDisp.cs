using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.DialogLogic.Utils;
using LINQPadExtras.Scripting_Batcher.Panels;
using LINQPadExtras.Scripting_Batcher.Utils;
using LINQPadExtras.Utils.Exts;
using PowRxVar;

namespace LINQPadExtras.Scripting_Batcher;

interface ICmdDisp
{
	void Log(string str);
	void LogError(string str);
	void LogException(Exception ex);
	bool AskConfirmation(string str, CancellationToken cancelToken);
	void AddCmdPanel(Control root);
	void ShowCmd(string exe, string args);
	void ShowArtifacts(List<string> artifacts);
}

class ConCmdDisp : ICmdDisp
{
	public void Log(string str) => str.Dump();

	public void LogError(string str) => Console.Error.WriteLine(str);

	public void LogException(Exception ex) => Console.Error.WriteLine(ex.ToString());

	public bool AskConfirmation(string str, CancellationToken cancelToken)
	{
		Console.WriteLine($"{str} (y/n)");
		var answer = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
		return answer == "y";
	}

	public void AddCmdPanel(Control root)
	{

	}

	public void ShowCmd(string exe, string args)
	{
		Console.WriteLine($"{exe} {args}");
	}

	public void ShowArtifacts(List<string> artifacts)
	{
		Log(" ");
		Log($"{artifacts.Count} artifacts:");
		for (var i = 0; i < artifacts.Count; i++)
		{
			var artifact = artifacts[i];
			Log($"  [{i}]: {artifact.GetArtifactLink()}");
		}
	}
}

class GuiCmdDisp : ICmdDisp
{
	private IDCWrapper dc = null!;

	public void HookDC(IDCWrapper pDc) => dc = pDc;

	public void Log(string str) => dc.AppendContent(new Div(new Span(str)).StyleLogLine());

	public void LogError(string str)
	{
		dc.AppendContent("\n");
		var msgDiv = new Div(new Span(str)).SetForeColor(BatcherConsts.OperationCancelledMessageColor).Set("font-weight", "bold");
		dc.AppendContent(msgDiv);
		dc.AppendContent("\n");
	}

	public void LogException(Exception ex) => dc.AppendContent(ex);

	public bool AskConfirmation(string str, CancellationToken cancelToken)
	{
		dc.AppendContent("\n");
		var msgDiv = new Div(new Span(str)).SetForeColor(BatcherConsts.ConfirmationMessageColor);
		dc.AppendContent(msgDiv);
		dc.AppendContent("\n");

		var btnYes = new Button("Yes") { CssClass = "modal-yesno-button modal-yesno-button-yes" }.MultiThread();
		var btnNo = new Button("No") { CssClass = "modal-yesno-button modal-yesno-button-no" }.MultiThread();
		var btnDiv = new Div(btnYes, btnNo) { CssClass = "modal-yesno-div" };
		dc.AppendContent(btnDiv);

		var answer = false;

		using var d = new Disp();
		using var slim = new ManualResetEventSlim();
		btnNo.WhenClick().Subscribe(_ =>
		{
			slim.Set();
		}).D(d);
		btnYes.WhenClick().Subscribe(_ =>
		{
			answer = true;
			slim.Set();
		}).D(d);

		try
		{
			slim.Wait(cancelToken);
		}
		finally
		{
			btnYes.Enabled = btnNo.Enabled = false;
		}

		return answer;
	}

	public void AddCmdPanel(Control root)
	{
		dc.AppendContent(root);
	}

	public void ShowCmd(string exe, string args)
	{
		using var panel = new CmdPanel(exe, args, CmdPanelMode.LogCmdOnly);
		dc.AppendContent(panel.Root);
	}

	public void ShowArtifacts(List<string> artifacts)
	{
		Log(" ");
		Log($"{artifacts.Count} artifacts:");
		for (var i = 0; i < artifacts.Count; i++)
		{
			var artifact = artifacts[i];
			dc.AppendContent(Util.HorizontalRun(true,
				new Div(new Span($"  [{i}]: ")),
				new Hyperlinq(artifact.GetArtifactLink(), artifact)
			));
		}
	}
}