using AngleSharp.Html;
using AngleSharp.Html.Parser;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.PageLogic;
using LINQPadExtras.PageServing.PageLogic.Transformers;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;

namespace LINQPadExtras.PageServing.Utils.HtmlUtils;

public static class HtmlDocUtils
{
	public static void SavePage(string folder)
	{
		var replier = new ServerReplier();

		var mutator = new PageMutator(

			LINQPadServer.GetSaveTransformers().Concat(
				Tr(
					new ImageFixerTransformer(replier),
					
					new CssScriptExtractionTransformer(replier, o =>
					{
						o.PredefinedNames.AddRange(new[] { "linqpad", "reset", "look" });
						o.GroupRestInLastName = true;
					}),

					new JsScriptExtractionTransformer(replier, o =>
					{
						o.PredefinedNames.Add("linqpad");
					})
				)
			)
			.ToArray()
		);

		PageSaver.Save(folder, mutator, replier);
	}

	private static ITransformer[] Tr(params ITransformer[] transformers) => transformers;


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
}