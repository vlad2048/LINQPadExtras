<Query Kind="Program">
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>AngleSharp.Html</Namespace>
</Query>

void Main()
{
	Html.GetBody().Dump();
}


static class Html
{
	public static string GetBody()
	{
		var body = ((string)Util.InvokeScript(true, "eval", "document.head.outerHTML")).ExtractInnerHtml();
		return body.Beautify();
	}
	
	
	
	private static string ExtractInnerHtml(this string html)
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(html);
		return doc.DocumentNode.ChildNodes[0].InnerHtml.Trim();
	}
	
	private static string Beautify(this string html)
	{
		var parser = new HtmlParser();
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
}