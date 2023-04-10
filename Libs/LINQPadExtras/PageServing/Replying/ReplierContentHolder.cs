using System.Diagnostics.CodeAnalysis;
using LINQPadExtras.PageServing.Replying.Structs;

namespace LINQPadExtras.PageServing.Replying;

class ReplierContentHolder
{
	private readonly Dictionary<string, ContentNfo> contentMap = new();
	public void AddContent(string link, ContentNfo content) => contentMap[link] = content;
	public bool TryGetContent(string link, [NotNullWhen(true)]out ContentNfo? content) => contentMap.TryGetValue(link, out content);

	public string[] GetAllFiles(string folder) =>
		contentMap.Keys
			.Select(e => MkFile(folder, e))
			.ToArray();

	public void SaveToFolder(string folder)
	{
		var subFolders = GetAllFiles(folder)
			.Select(e => Path.GetDirectoryName(e)!)
			.Distinct()
			.ToArray();
		foreach (var subFolder in subFolders)
			FixSubFolder(subFolder);

		foreach (var (link, (_, contentType, str)) in contentMap) {
			var file = MkFile(folder, link);
			switch (contentType)
			{
				case ContentType.String:
					File.WriteAllText(file, str);
					break;
				case ContentType.File:
					File.Copy(str, file);
					break;
				default:
					throw new ArgumentException();
			}
		}
	}

	private static void FixSubFolder(string subFolder)
	{
		if (!Directory.Exists(subFolder))
		{
			Directory.CreateDirectory(subFolder);
			return;
		}

		var files = Directory.GetFiles(subFolder);
		foreach (var file in files)
			File.Delete(file);
	}

	private static string MkFile(string folder, string link) =>
		Path.Combine(folder, link.Replace("/", @"\"));
}