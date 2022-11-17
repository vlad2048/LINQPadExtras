using System.Windows.Forms;
using LINQPadExtras.Utils.Exts;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace LINQPadExtras.CmdRunning;

class CmdState
{
	private readonly IRwVar<bool> isInCmd = Var.Make(false);
	private readonly List<string> cmdLines = new();

	public string? CurFolder { get; private set; }
	public List<string> Artifacts { get; } = new();
	public bool DryRun { get; set; }
	public IRoVar<bool> IsInCmd => isInCmd;

	public void Clear(bool dryRun)
	{
		CurFolder = null;
		Artifacts.Clear();
		DryRun = dryRun;
		cmdLines.Clear();
		isInCmd.V = true;
	}

	public void End()
	{
		DryRun = false;
		isInCmd.V = false;
	}

	public void AddCmdLine(string line) => cmdLines.Add(line);

	public void CopyCmdLinesToClipboard() => Clipboard.SetText(cmdLines.JoinText(Environment.NewLine) + Environment.NewLine);



	public void AddArtifact(string artifact) => Artifacts.Add(artifact);

	public string ChangeDir(string exeFile, string workingDir)
	{
		var exeFileFolderN = Path.GetDirectoryName(exeFile).ToStrNull();
		var workingFolderN = workingDir.ToStrNull();

		var curFolderDesired = workingFolderN ?? exeFileFolderN ?? CurFolder;

		if (curFolderDesired != CurFolder)
		{
			CurFolder = curFolderDesired;
			if (CurFolder != null)
				Con.ShowCmd("cd", $"/d {CurFolder.QuoteIFN()}");
		}

		return (exeFileFolderN != null && exeFileFolderN == CurFolder) switch
		{
			true => Path.GetFileName(exeFile),
			false => exeFile
		};
	}
}

static class CmdStateExt
{
	public static string? ToStrNull(this string? s) => s switch
	{
		null => null,
		"" => null,
		_ => s
	};
}