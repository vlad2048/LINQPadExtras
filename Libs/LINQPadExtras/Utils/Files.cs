namespace LINQPadExtras.Utils;

public static class Files
{
	public static string[] FindRecursively(string folder, string pattern)
	{
		var list = new List<string>();

		void Recurse(string curFolder)
		{
			list.AddRange(Directory.GetFiles(curFolder, pattern));
			var subFolders = Directory.GetDirectories(curFolder);
			foreach (var subFolder in subFolders)
				Recurse(subFolder);
		}

		Recurse(folder);
		return list.ToArray();
	}

	internal static string OmitExeFolderIfInCurDir(string exeFile, string? curDir) => curDir switch
	{
		null => exeFile,
		not null => (Path.GetDirectoryName(exeFile) == curDir) switch
		{
			false => exeFile,
			true => Path.GetFileName(exeFile)
		}
	};
}