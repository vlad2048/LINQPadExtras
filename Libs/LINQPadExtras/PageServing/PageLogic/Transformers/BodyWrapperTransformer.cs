using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class BodyWrapperTransformer : ITransformer
{
	private readonly Func<string, string> wrapper;

	public BodyWrapperTransformer(Func<string, string> wrapper)
	{
		this.wrapper = wrapper;
	}

	public void Apply(HtmlNode root)
	{
		var body = root.FindNodeByName("body");
		var bodyHtml = body.InnerHtml;
		var bodyHtmlWrapped = wrapper(bodyHtml);
		body.InnerHtml = bodyHtmlWrapped;
	}
}