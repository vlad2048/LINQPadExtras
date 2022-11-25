using System.Text;
using CliWrap;
using LINQPad;
using LINQPad.Controls;
using LINQPadExtras.DialogLogic.Utils;
using LINQPadExtras.Scripting_Batcher.Panels;
using LINQPadExtras.Scripting_Batcher.Utils;
using LINQPadExtras.Utils;
using LINQPadExtras.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;
using Clipboard = System.Windows.Forms.Clipboard;

namespace LINQPadExtras.Scripting_Batcher;


public interface ICmd
{
	void Log(string str);
	void AddArtifact(string artifact);
	void Run(string exeFile, params string[] args);
	void RunLeaveOpen(string exeFile, params string[] args);
	void Cancel(string message);
	void AskConfirmation(string message, Action? onCancel = null);
	void Cd(string folder);
	void DeleteFile(string file);
	void MakeFolder(string folder);
	void DeleteFolder(string folder);
	void EmptyFolder(string folder);
}


class Cmd : ICmd, IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly bool dryRun;
	private readonly CancellationTokenSource cancelSource;
	private readonly CancellationToken cancelToken;
	private readonly List<string> cmdLines = new();
	private readonly List<string> artifacts = new();
	private string? curDir;

	

	public Cmd(bool dryRun)
	{
		this.dryRun = dryRun;
		cancelSource = new CancellationTokenSource().D(d);
		cancelToken = cancelSource.Token;
	}



	// ***********************************
	// * Public, only used in Batcher.cs *
	// ***********************************
	public bool IsCancelled => cancelToken.IsCancellationRequested;
	public bool LeaveOpenAfter { get; private set; }
	public IDCWrapper DC { get; set; } = null!;
	public void Cancel()
	{
		cancelSource.Cancel();
		cancelToken.ThrowIfCancellationRequested();
	}

	public void CopyToClipboard()
	{
		var str = cmdLines.JoinText(Environment.NewLine) + Environment.NewLine;
		var thread = new Thread(() => Clipboard.SetText(str));
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start(); 
		thread.Join();
	}

	public void LogArtifacts()
	{
		if (IsCancelled) return;

		Log(" ");
		Log($"{artifacts.Count} artifacts:");
		for (var i = 0; i < artifacts.Count; i++)
		{
			var artifact = artifacts[i];
			DC.AppendContent(Util.HorizontalRun(true,
				new Div(new Span($"  [{i}]: ")),
				new Hyperlinq(artifact.GetArtifactLink(), artifact)
			));
		}
	}



	// **********
	// * Public *
	// **********
	public void Log(string str)
	{
		var div = new Div(new Span(str)).StyleLogLine();
		DC.AppendContent(div);
	}

	public void AddArtifact(string artifact)
	{
		if (IsCancelled) return;
		artifacts.Add(artifact);
	}

	public void Run(string exeFile, params string[] args) => Run(false, exeFile, args);

	public void RunLeaveOpen(string exeFile, params string[] args) => Run(true, exeFile, args);

	public void Cancel(string message)
	{
		LeaveOpenAfter = true;
		DC.AppendContent("\n");
		var msgDiv = new Div(new Span(message)).SetForeColor(BatcherConsts.OperationCancelledMessageColor).Set("font-weight", "bold");
		DC.AppendContent(msgDiv);
		DC.AppendContent("\n");
		Cancel();
	}

	public void AskConfirmation(string message, Action? onCancel)
	{
		DC.AppendContent("\n");
		var msgDiv = new Div(new Span(message)).SetForeColor(BatcherConsts.ConfirmationMessageColor);
		DC.AppendContent(msgDiv);
		DC.AppendContent("\n");

		var btnYes = new Button("Yes") { CssClass = "modal-yesno-button modal-yesno-button-yes" }.MultiThread();
		var btnNo = new Button("No") { CssClass = "modal-yesno-button modal-yesno-button-no" }.MultiThread();
		var btnDiv = new Div(btnYes, btnNo) { CssClass = "modal-yesno-div" };
		DC.AppendContent(btnDiv);

		using var slim = new ManualResetEventSlim().D(d);
		btnNo.WhenClick().Subscribe(_ =>
		{
			onCancel?.Invoke();
			Cancel();
		}).D(d);
		btnYes.WhenClick().Subscribe(_ =>
		{
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
	}

	private void Run(bool leaveOpen, string exeFile, params string[] args)
	{
		if (IsCancelled) return;
		exeFile = Files.OmitExeFolderIfInCurDir(exeFile, curDir);
		var cmd = Cli.Wrap(exeFile)
			.WithWorkingDirectoryOpt(curDir)
			.WithArguments(args)
			.WithValidation(CommandResultValidation.ZeroExitCode);
		cmdLines.Add($"{exeFile} {cmd.Arguments}");
		var mode = dryRun switch
		{
			false => leaveOpen switch
			{
				false => CmdPanelMode.Normal,
				true => CmdPanelMode.LeaveOpen,
			},
			true => CmdPanelMode.LogCmdOnly,
		};
		var panel = new CmdPanel(exeFile, cmd.Arguments, mode).D(d);
		DC.AppendContent(panel.Root);
		if (dryRun) return;

		try
		{
			var sbOut = new StringBuilder();

			cmd
				.WithStandardOutputPipe(
					PipeTarget.Merge(
						PipeTarget.ToDelegate(panel.StdOut),
						PipeTarget.ToStringBuilder(sbOut)
					)
				)
				.WithStandardErrorPipe(PipeTarget.ToDelegate(panel.StdErr))
				.WithValidation(CommandResultValidation.ZeroExitCode)
				.ExecuteAsync(cancelToken)
				.GetAwaiter()
				.GetResult();

			panel.Complete(true);

			var str = sbOut.ToString();
		}
		catch (CommandExecutionException ex)
		{
			panel.Complete(false);
			DC.AppendContent(ex);
			throw;
		}
		catch (OperationCanceledException)
		{
			Log("Cancelled");
			throw;
		}
	}

	public void Cd(string folder)
	{
		if (IsCancelled) return;
		if (curDir == folder) return;
		ShowAndAddCmd("cd", "/d", folder);
		curDir = folder;
	}

	public void DeleteFile(string file)
	{
		if (IsCancelled) return;
		if (!File.Exists(file)) return;
		ShowAndAddCmd("del", "/q", file);
		if (dryRun) return;

		File.Delete(file);
	}

	public void MakeFolder(string folder)
	{
		if (IsCancelled) return;
		if (!Directory.Exists(folder)) return;
		ShowAndAddCmd("mkdir", folder);
		if (dryRun) return;

		Directory.CreateDirectory(folder);
	}

	public void DeleteFolder(string folder)
	{
		if (IsCancelled) return;
		if (!Directory.Exists(folder)) return;
		ShowAndAddCmd("rmdir", "/s", "/q", folder);
		if (dryRun) return;

		Directory.Delete(folder, true);
	}

	public void EmptyFolder(string folder)
	{
		if (IsCancelled) return;
		if (!Directory.Exists(folder)) return;
		if (Directory.GetFileSystemEntries(folder).Length == 0) return;
		DeleteFolder(folder);
		MakeFolder(folder);
	}



	private void ShowAndAddCmd(string exeFile, params string[] args)
	{
		var cmd = Cli.Wrap(exeFile)
			.WithArguments(args);
		cmdLines.Add($"{exeFile} {cmd.Arguments}");
		using var panel = new CmdPanel(exeFile, cmd.Arguments, CmdPanelMode.LogCmdOnly);
		DC.AppendContent(panel.Root);
	}
}
