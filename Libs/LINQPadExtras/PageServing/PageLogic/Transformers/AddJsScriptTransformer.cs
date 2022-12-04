using HtmlAgilityPack;
using LINQPadExtras.PageServing.Components;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class AddJsScriptTransformer : ITransformer
{
	private readonly ServerReplier replier;
	private readonly string scriptName;
	private readonly string scriptContent;

	public AddJsScriptTransformer(ServerReplier replier, string scriptName, string scriptContent)
	{
		this.replier = replier;
		this.scriptName = scriptName;
		this.scriptContent = scriptContent;
	}

	public void Apply(HtmlNode root)
	{
		var headChildren = root.FindNodeByName("head").ChildNodes;
		var link = $"js/{scriptName}.js";
		var dom = $"""<script src="{link}"></script>""";
		var domNode = HtmlNode.CreateNode(dom);
		headChildren.Append(domNode);

		replier.AddContentString(link, new ContentNfo(ReplyType.ScriptJs, scriptContent));
	}
}