using HtmlAgilityPack;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class ApplyTweaksTransformer : ITransformer
{
	private readonly Tweaks tweaks;

	public ApplyTweaksTransformer(Tweaks tweaks)
	{
		this.tweaks = tweaks;
	}

	public void Apply(HtmlNode root)
	{
		root.ForEachNode(node =>
		{
			var tweakAction = tweaks.GetTweakOpt(node.Id);
			tweakAction?.Invoke(node);
		});
	}
}