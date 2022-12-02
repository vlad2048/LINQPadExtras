<Query Kind="Program">
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>AngleSharp.Html</Namespace>
</Query>

void Main()
{
	var link = new Hyperlink("link");
	var ctrl = link.HtmlElement;
	ctrl.SetAttribute("href", null);
	link.Dump();
	
	Util.FixedFont(HtmlPageReadingUtils.GetBody()).Dump();
}



static class HtmlPageReadingUtils
{
	public static string GetHead() => ((string)Util.InvokeScript(true, "eval", "document.head.outerHTML")).ExtractInnerHtml();
	
	public static string GetBody() => ((string)Util.InvokeScript(true, "eval", "document.body.outerHTML")).ExtractInnerHtml();


	private static string Beautify(this string html, bool body)
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