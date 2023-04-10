using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Replying;
using LINQPadExtras.PageServing.Replying.Structs;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;

class AddJsScriptTransformer : ITransformer
{
	private readonly ReplierContentHolder contentHolder;
	private readonly string scriptName;
	private readonly string scriptContent;

	public AddJsScriptTransformer(ReplierContentHolder contentHolder, string scriptName, string scriptContent)
	{
		this.contentHolder = contentHolder;
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

		contentHolder.AddContent(link, new ContentNfo(ReplyType.ScriptJs, ContentType.String, scriptContent));
	}
}