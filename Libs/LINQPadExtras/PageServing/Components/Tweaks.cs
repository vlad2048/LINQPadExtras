using HtmlAgilityPack;

namespace LINQPadExtras.PageServing.Components;

class Tweaks
{
	private readonly Dictionary<string, Action<HtmlNode>> tweakMap = new();

	public void SetTweak(string id, Action<HtmlNode> action) => tweakMap[id] = action;

	public Action<HtmlNode>? GetTweakOpt(string id) => tweakMap.TryGetValue(id, out var action) switch
	{
		true => action,
		false => null
	};
}