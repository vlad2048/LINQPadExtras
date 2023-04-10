using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Utils.HtmlUtils;
using PowBasics.CollectionsExt;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class RemoveCssScriptTransformer : ITransformer
{
	private readonly HashSet<string> cssContentsToRemove;

	public RemoveCssScriptTransformer(HashSet<string> cssContentsToRemove)
	{
		this.cssContentsToRemove = cssContentsToRemove;
	}

	public void Apply(HtmlNode root)
	{
		var headChildren = root.FindNodeByName("head").ChildNodes;
		var nodes = headChildren.WhereToArray(e => e.IsCssNode() && cssContentsToRemove.Any(f => e.InnerText == f));
		nodes.Delete();
	}
}