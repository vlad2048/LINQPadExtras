using Path = System.IO.Path;

namespace LINQPadExtras;

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

	public static string ChangeFolder(this string file, string folder) => Path.Combine(folder, Path.GetFileName(file));

	public static string AppendSuffix(this string file, string suffix)
	{
		var folder = Path.GetDirectoryName(file)!;
		var name = Path.GetFileNameWithoutExtension(file);
		var ext = Path.GetExtension(file);
		return Path.Combine(folder, $"{name}{suffix}{ext}");
	}
}