using AngleSharp.Html;
using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using LINQPad;

namespace LINQPadExtras.PageServing.Utils.HtmlUtils;

static class HtmlPageReadingUtils
{
	public static string GetHead() => ((string)Util.InvokeScript(true, "eval", "document.head.outerHTML")).ExtractInnerHtml();
	
	public static string GetBody() => ((string)Util.InvokeScript(true, "eval", "document.body.outerHTML")).ExtractInnerHtml();


	public static string Beautify(this string html, bool body)
	{
		var parser = new HtmlParser();
		if (body)
		{
			var dom = parser.ParseDocument("<html><body></body></html>");
			var doc = parser.ParseFragment(html, dom.Body!);
			using var writer = new StringWriter();
			doc.ToHtml(writer, new PrettyMarkupFormatter
			{
				Indentation = "\t",
				NewLine = "\n",
			});
			var formattedHtml = writer.ToString();
			return formattedHtml;
		}
		else
		{
			var doc = parser.ParseDocument(html);
			using var writer = new StringWriter();
			doc.ToHtml(writer, new PrettyMarkupFormatter
			{
				Indentation = "\t",
				NewLine = "\n",
			});
			var formattedHtml = writer.ToString();
			return formattedHtml;
		}
	}

	private static string ExtractInnerHtml(this string html)
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(html);
		return doc.DocumentNode.ChildNodes[0].InnerHtml.Trim().Beautify(true);
	}
}