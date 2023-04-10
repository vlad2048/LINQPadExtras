using HtmlAgilityPack;
using LINQPad;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.PageLogic;

record PageRefreshNfo(
	string HtmlStyle,
	string Head,
	string Body
);

class PageMutator
{
	private readonly ITransformer[] transformers;

	public PageMutator(params ITransformer[] transformers)
	{
		this.transformers = transformers;
	}

	public string GetPage()
	{
		var htmlStyle = PageGetUtils.GetHtmlStyle();
		var head = PageGetUtils.GetHead();
		var body = PageGetUtils.GetBody();

		var htmlStyleStr = string.IsNullOrWhiteSpace(htmlStyle) switch
		{
			true => string.Empty,
			false => $@" style=""{htmlStyle}"""
		};

		var html = $"""
			<!DOCTYPE html>
			<html{htmlStyleStr}>
				<head>
					{head}
				</head>
				<body>
					{body}
				</body>
			</html>
			""";
		var doc = new HtmlDocument();
		doc.LoadHtml(html);
		var root = doc.DocumentNode;

		foreach (var transformer in transformers)
			transformer.Apply(root);

		return root.OuterHtml.Beautify(false);
	}

	public PageRefreshNfo GetPageRefresh()
	{
		var html = GetPage();
		var doc = new HtmlDocument();
		doc.LoadHtml(html);
		var root = doc.DocumentNode;

		var htmlNode = root.FindNodeByName("html");
		var htmlStyle = htmlNode.GetAttrOpt("style") ?? string.Empty;
		var head = root.FindNodeByName("head").InnerHtml;
		var bodyNode = root.FindNodeById(FrontendScripts.MainDivId);

		//var connServerInBody = bodyNode.ChildNodes.Where(e => e.Id == )

		return new PageRefreshNfo(htmlStyle, head, bodyNode.InnerHtml);
	}
}

public static class PageGetUtils
{
	public static string GetHtmlStyle() => (string)Util.InvokeScript(true, "eval", "document.querySelector('html').style.cssText");
	public static string GetHead() => ((string)Util.InvokeScript(true, "eval", "document.head.outerHTML")).ExtractInnerHtml();
	public static string GetBody() => ((string)Util.InvokeScript(true, "eval", "document.body.outerHTML")).ExtractInnerHtml();
}

static class PageMutatorExt
{
	public static string ExtractInnerHtml(this string html)
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(html);
		return doc.DocumentNode.ChildNodes[0].InnerHtml.Trim().Beautify(true);
	}
}