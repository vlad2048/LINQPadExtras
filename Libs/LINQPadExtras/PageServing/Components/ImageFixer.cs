using HtmlAgilityPack;
using LINQPadExtras.PageServing.Utils.Exts;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.Components;

class ImageFixer
{
	private const string LinkPrefix = "images/";

	private readonly Dictionary<string, string> file2link = new();
	private readonly Dictionary<string, string> link2file = new();

	public void TweakBody(HtmlDocument doc)
	{
		var nodes = doc.DocumentNode.FindNodes(e => e.Name == "img" && e.GetAttrOpt("src")?.StartsWith("file:///") == true);
		foreach (var node in nodes)
		{
			var file = node.GetAttrOpt("src")!.Link2File();
			var link = GetLink(file);
			node.SetAttr("src", link);
		}
	}

	public static bool IsImageUrl(string url) => url.StartsWith(LinkPrefix);
	public string GetFileFromUrl(string url) => link2file[url];

	public void WriteAllToFolder(string folder)
	{
		foreach (var (link, file) in link2file)
		{
			var imageFile = Path.Combine(folder, link.Link2Path()).CreateFolderForFileIFN();
			File.Copy(file, imageFile, true);
		}
	}


	private string GetLink(string file)
	{
		if (file2link.TryGetValue(file, out var link)) return link;
		link = MakeLink(file);
		file2link[file] = link;
		link2file[link] = file;
		return link;
	}


	private string MakeLink(string file)
	{
		var nameBase = Path.GetFileNameWithoutExtension(file);
		var nameExt = Path.GetExtension(file);
		var link = $"{LinkPrefix}{nameBase}{nameExt}";
		var idx = 1;
		while (link2file.ContainsKey(link))
			link = $"{LinkPrefix}{nameBase}({idx++}){nameExt}";
		return link;
	}
}



static class ImageTweakerExt
{
	private const string Prefix = "file:///";

	public static string Link2File(this string s) => s
		.RemovePrefix()
		.RemoveQueryArgs()
		.Replace("/", @"\");
		
	private static string RemovePrefix(this string s)
	{
		if (!s.StartsWith(Prefix)) throw new ArgumentException();
		return s[Prefix.Length..];
	}
	private static string RemoveQueryArgs(this string s)
	{
		var idx = s.IndexOf('?');
		return (idx != -1) switch
		{
			true => s[..idx],
			false => s
		};
	}
}