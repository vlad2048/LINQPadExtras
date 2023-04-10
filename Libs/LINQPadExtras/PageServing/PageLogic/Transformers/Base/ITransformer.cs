using HtmlAgilityPack;

namespace LINQPadExtras.PageServing.PageLogic.Transformers.Base;

public interface ITransformer
{
	void Apply(HtmlNode root);
}