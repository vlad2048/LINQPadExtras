using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Utils.HtmlUtils;
using PowBasics.CollectionsExt;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class RemoveNodeTransformer : ITransformer
{
	private readonly string id;

	public RemoveNodeTransformer(string id)
	{
		this.id = id;
	}

	public void Apply(HtmlNode root)
	{
		root.FindNodes(e => e.Id == id)
			.ForEach(node => node.Remove());
	}
}