using LINQPadExtras.PageServing.PageLogic;
using LINQPadExtras.PageServing.Replying;
using PowBasics.CollectionsExt;

namespace LINQPadExtras.PageServing.Utils;

static class PageSaver
{
	private static readonly string[] defaultFilesToKeep =
	{
		"index.html",
		"gulpfile.js",
		"package-lock.json",
		"package.json",
		"run.bat",
	};

	private static readonly string[] defaultFoldersToKeep =
	{
		".vscode",
		"node_modules",
	};


	public static void Save(string folder, PageMutator mutator, ReplierContentHolder contentHolder)
	{
		if (!Directory.Exists(folder)) return;

		var replierFiles = contentHolder.GetAllFiles(folder);

		var filesKeep = defaultFilesToKeep.Select(e => Path.Combine(folder, e))
			.Concat(replierFiles).ToArray();
		var foldersKeep = defaultFoldersToKeep.Select(e => Path.Combine(folder, e))
			.Concat(replierFiles.Select(e => Path.GetDirectoryName(e)!).Distinct()).ToArray();
		DeleteBut(folder, filesKeep, foldersKeep);

		var page = mutator.GetPage();
		var indexFile = Path.Combine(folder, "index.html");
		File.WriteAllText(indexFile, page);

		contentHolder.SaveToFolder(folder);
	}

	private static void DeleteBut(string root, string[] filesToKeep, string[] foldersToKeep)
	{
		var filesToDelete = Directory.GetFiles(root).WhereNotToArray(filesToKeep.Contains);
		var foldersToDelete = Directory.GetDirectories(root).WhereNotToArray(foldersToKeep.Contains);

		foreach (var file in filesToDelete)
			File.Delete(file);

		foreach  (var folder in foldersToDelete)
			Directory.Delete(folder, true);
	}
}


/*using LINQPadExtras.PageServing.Utils.Exts;

namespace LINQPadExtras.PageServing.Utils;

class FileSet
{
	private static readonly string[] filesToKeep =
	{
		"gulpfile.js",
		"package-lock.json",
		"package.json",
		"run.bat",
	};

	private static readonly string[] foldersToKeep =
	{
		".vscode",
		"node_modules",
	};

	public List<string> FilesToKeep { get; } = filesToKeep.ToList();
}

static class PageSaver
{

	public static bool CleanFolder(string folder)
	{
		if (string.IsNullOrEmpty(folder)) return false;
		folder.CreateFolderIFN();

		var filesToDelete = Directory.GetFiles(folder);
		foreach (var fileToDelete in filesToDelete)
		{
			if (!filesToKeep.Contains(Path.GetFileName(fileToDelete)))
				File.Delete(fileToDelete);
		}

		var foldersToDelete = Directory.GetDirectories(folder);
		foreach (var folderToDelete in foldersToDelete)
		{
			if (!foldersToKeep.Contains(Path.GetFileName(folderToDelete)))
				Directory.Delete(folderToDelete, true);
		}

		return true;
	}
}*/