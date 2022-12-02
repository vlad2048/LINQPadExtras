<Query Kind="Program">
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>AngleSharp.Html</Namespace>
  <Namespace>HtmlAgilityPack</Namespace>
</Query>

void Main()
{
	var headHtml = HtmlPageReadingUtils.GetHead();
	var doc = new HtmlDocument();
	doc.LoadHtml(headHtml);
	
	var root = doc.DocumentNode;
	root.ChildNodes.Select(e => e.Name).Dump();
	var scriptNodes = root.ChildNodes.Where(e => e.Name == "script").ToArray();
	//var s = scriptNodes[0];
	
	//scriptNodes.Length.Dump();
	scriptNodes.Select(s => s.Attributes.ToDictionary(e => e.Name, e => e.Value)).Dump();
	
	//Util.FixedFont(doc.DocumentNode.OuterHtml).Dump();
}



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