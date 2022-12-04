using System.Net;
using System.Text;
using LINQPadExtras.PageServing.Utils.Exts;

namespace LINQPadExtras.PageServing.Components;

enum ReplyType
{
	None,
	Html,
	ScriptJs,
	ScriptCss,
	ImagePng,
	ImageJpg,
	ImageSvg,
}

record Reply(
	byte[] Data,
	ReplyType Type
)
{
	public async Task Write(HttpListenerResponse response)
	{
		response.ContentType = Type switch
		{
			ReplyType.Html => "text/html",
			ReplyType.ScriptJs => "text/javascript",
			ReplyType.ScriptCss => "text/css",
			ReplyType.ImagePng => "image/png",
			ReplyType.ImageJpg => "image/jpeg",
			ReplyType.ImageSvg => "image/svg+xml",
			_ => throw new ArgumentException()
		};
		response.ContentEncoding = Type switch
		{
			ReplyType.Html => Encoding.UTF8,
			ReplyType.ScriptJs => Encoding.UTF8,
			ReplyType.ScriptCss => Encoding.UTF8,
			ReplyType.ImagePng => Encoding.Default,
			ReplyType.ImageJpg => Encoding.Default,
			ReplyType.ImageSvg => Encoding.UTF8,
			_ => throw new ArgumentException()
		};
		response.ContentLength64 = Data.LongLength;
		await response.OutputStream.WriteAsync(Data, 0, Data.Length);
	}
}

record ContentNfo(ReplyType Type, string StrOrFile);

class ServerReplier
{
	private readonly Dictionary<string, ContentNfo> contentStringMap = new();
	private readonly Dictionary<string, ContentNfo> contentFileMap = new();

	public Func<string> HtmlPageFun { get; set; } = null!;
	public void AddContentString(string link, ContentNfo content) => contentStringMap[link] = content;
	public void AddContentFile(string link, ContentNfo file) => contentFileMap[link] = file;

	public async Task<Reply> Reply(string url)
	{
		url = url.RemoveLeadingSlash();

		if (url == string.Empty)
		{
			var pageHtml = HtmlPageFun();
			return new Reply(pageHtml.ToBytes(), ReplyType.Html);
		}

		if (contentStringMap.TryGetValue(url, out var content))
		{
			return new Reply(content.StrOrFile.ToBytes(), content.Type);
		}

		if (contentFileMap.TryGetValue(url, out content))
		{
			var str = await File.ReadAllBytesAsync(content.StrOrFile);
			return new Reply(str, content.Type);
		}

		return new Reply(Array.Empty<byte>(), ReplyType.None);
	}

	public string[] GetAllFiles(string folder) =>
		contentStringMap.Keys.Concat(contentFileMap.Keys)
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

		foreach (var (link, content) in contentStringMap)
		{
			var file = MkFile(folder, link);
			File.WriteAllText(file, content.StrOrFile);
		}

		foreach (var (link, content) in contentFileMap)
		{
			var file = MkFile(folder, link);
			File.Copy(content.StrOrFile, file);
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

static class ServerReplierExt
{
	public static byte[] ToBytes(this string str) => Encoding.UTF8.GetBytes(str);

	/*
		public static Encoding GetEncodingForReplyType(this ReplyType type) => type switch
		{
			ReplyType.Html => Encoding.UTF8,
			ReplyType.ScriptJs => Encoding.UTF8,
			ReplyType.ScriptCss => Encoding.UTF8,
			ReplyType.ImagePng => Encoding.Default,
			ReplyType.ImageJpg => Encoding.Default,
			ReplyType.ImageSvg => Encoding.UTF8,
			_ => throw new ArgumentException()
		};
	 */
}