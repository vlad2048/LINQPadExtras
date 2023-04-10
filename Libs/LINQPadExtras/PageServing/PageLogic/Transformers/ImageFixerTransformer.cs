using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Replying;
using LINQPadExtras.PageServing.Replying.Structs;
using LINQPadExtras.PageServing.Utils.HtmlUtils;
using PowBasics.CollectionsExt;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class ImageFixerTransformer : ITransformer
{
	private readonly ReplierContentHolder contentHolder;

	public ImageFixerTransformer(ReplierContentHolder contentHolder)
	{
		this.contentHolder = contentHolder;
	}

	public void Apply(HtmlNode root)
	{
		root.FindNodeByName("body")
			.FindNodes(e => e.IsImgNode())
			.ForEach(e =>
			{
				var file = e.GetAttrOpt("src")!.HtmlFile2File();
				var link = MkImgLink(file);
				e.SetAttr("src", link);

				var replyType = GetReplyTypeFromExt(file);
				contentHolder.AddContent(link, new ContentNfo(replyType, ContentType.File, file));
			});
	}

	private static string MkImgLink(string file) => $"images/{Path.GetFileName(file)}";

	private static ReplyType GetReplyTypeFromExt(string file)
	{
		var ext = Path.GetExtension(file).ToLowerInvariant();
		return ext switch
		{
			".png" => ReplyType.ImagePng,
			".jpg" => ReplyType.ImageJpg,
			".jpeg" => ReplyType.ImageJpg,
			".svg" => ReplyType.ImageSvg,
			_ => throw new ArgumentException()
		};
	}
}

static class ImageFixerTransformerExt
{
    private const string Prefix = "file:///";

	public static bool IsImgNode(this HtmlNode node) => node.Name == "img" && node.GetAttrOpt("src")?.StartsWith("file:///") == true;

	public static string HtmlFile2File(this string s) => s
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