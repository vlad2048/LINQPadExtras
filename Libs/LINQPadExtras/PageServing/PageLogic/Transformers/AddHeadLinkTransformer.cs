using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Replying;
using LINQPadExtras.PageServing.Replying.Structs;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class AddCssScriptTransformer : ITransformer
{
	private record CssScript(string Name, string Css);
	private readonly List<CssScript> scripts = new();

	private readonly ReplierContentHolder contentHolder;

	public AddCssScriptTransformer(ReplierContentHolder contentHolder)
	{
		this.contentHolder = contentHolder;
	}

	public void AddClientCss(string scriptName, string css) => scripts.Add(new CssScript(scriptName, css));

	public void Apply(HtmlNode root)
	{
		var headChildren = root.FindNodeByName("head").ChildNodes;

		foreach (var script in scripts)
		{
			var link = $"css/{script.Name}.css";
			var dom = $"""<link href="{link}" rel="stylesheet" type="text/css">""";
			var domNode = HtmlNode.CreateNode(dom);
			headChildren.Append(domNode);

			contentHolder.AddContent(link, new ContentNfo(ReplyType.ScriptCss, ContentType.String, script.Css));
		}

	}
}





/*

public record HeadLink(
	string Rel,
	string HRef,
	string? Type
)
{
	public static HeadLink Css(string href) => new("stylesheet", href, "test/css");
	public static HeadLink Icon(string href, string type) => new("icon", href, type);
	public static HeadLink Manifest(string href) => new("manifest", href, null);
}

class AddHeadLinkTransformer : ITransformer
{
	private readonly List<HeadLink> links = new();

	private readonly ReplierContentHolder contentHolder;

	public AddHeadLinkTransformer(ReplierContentHolder contentHolder)
	{
		this.contentHolder = contentHolder;
	}

	public void AddClientCss(HeadLink link) => links.Add(link);

	public void Apply(HtmlNode root)
	{
		var headChildren = root.FindNodeByName("head").ChildNodes;

		foreach (var link in links)
		{
			//var link = $"css/{script.Name}.css";
			var typeStr = link.Type switch
			{
				not null => $" type=\"{link.Type}\"",
				null => string.Empty
			};
			var dom = $"""<link rel="{link.Rel}" href="{link.HRef}"{typeStr}>""";
			var domNode = HtmlNode.CreateNode(dom);
			headChildren.Append(domNode);

			contentHolder.AddContent(link, new ContentNfo(ReplyType.ScriptCss, ContentType.String, script.Css));
		}

	}
}

*/