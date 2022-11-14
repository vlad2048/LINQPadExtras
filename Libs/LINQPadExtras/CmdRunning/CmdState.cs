using LINQPadExtras.Utils.Exts;

namespace LINQPadExtras.CmdRunning;

class CmdState
{
	public string? CurFolder { get; private set; }
	public List<string> Artifacts { get; } = new();

	public void Clear()
	{
		CurFolder = null;
		Artifacts.Clear();
	}

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