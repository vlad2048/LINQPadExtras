using HtmlAgilityPack;

namespace LINQPadExtras.PageServing.PageLogic.Transformers.Base;

interface ITransformer
{
	void Apply(HtmlNode root);
}