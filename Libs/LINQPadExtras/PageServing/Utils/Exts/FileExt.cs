namespace LINQPadExtras.PageServing.Utils.Exts;

static class FileExt
{
	public static string AddSuffixToFilename(this string file, string suffix)
	{
		var baseName = Path.GetFileNameWithoutExtension(file);
		var ext = Path.GetExtension(file);
		var folder = Path.GetDirectoryName(file)!;
		return Path.Combine(folder, $"{baseName}{suffix}{ext}");
	}

	public static string CreateFolderIFN(this string folder)
	{
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
		return folder;
	}

	public static string CreateFolderForFileIFN(this string file)
	{
		var folder = Path.GetDirectoryName(file)!;
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
		return file;
	}

	public static string RemoveLeadingSlash(this string s) => (s[0] == '/') switch
	{
		true => s[1..],
		false => s
	};

	public static string Link2Path(this string link) =>
		link
			.RemoveLeadingSlash()
			.Replace("/", @"\");
}