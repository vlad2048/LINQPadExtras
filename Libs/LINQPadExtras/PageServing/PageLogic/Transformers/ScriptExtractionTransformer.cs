using HtmlAgilityPack;
using LINQPadExtras.PageServing.PageLogic.Transformers.Base;
using LINQPadExtras.PageServing.Replying;
using LINQPadExtras.PageServing.Replying.Structs;
using LINQPadExtras.PageServing.Utils.HtmlUtils;
using PowBasics.CollectionsExt;

namespace LINQPadExtras.PageServing.PageLogic.Transformers;


class JsScriptExtractionTransformer : ScriptExtractionTransformer
{
	public JsScriptExtractionTransformer(
		ReplierContentHolder contentHolder,
		Action<ScriptExtractionTransformerOpt>? optFun = null
	)
		: base(
			contentHolder,
			optFun,
			node => node.IsJsNode(),
			link => $"""<script src="{link}"></script>""",
			ReplyType.ScriptJs,
			"js",
			"script",
			"js"
		)
	{
	}
}


class CssScriptExtractionTransformer : ScriptExtractionTransformer
{
	public CssScriptExtractionTransformer(
		ReplierContentHolder contentHolder,
		Action<ScriptExtractionTransformerOpt>? optFun = null
	)
		: base(
			contentHolder,
			optFun,
			node => node.IsCssNode(),
			link => $"""<link href="{link}" rel="stylesheet" type="text/css">""",
			ReplyType.ScriptCss,
			"css",
			"style",
			"css"
		)
	{
	}
}




class ScriptExtractionTransformerOpt
{
	public List<string> PredefinedNames { get; } = new();
	public bool GroupRestInLastName { get; set; }

	internal static ScriptExtractionTransformerOpt Build(Action<ScriptExtractionTransformerOpt>? optFun)
	{
		var opt = new ScriptExtractionTransformerOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

class ScriptExtractionTransformer : ITransformer
{
	private readonly ReplierContentHolder contentHolder;
	private readonly Func<HtmlNode, bool> predicate;
	private readonly Func<string, string> makeDom;
	private readonly ReplyType replyType;
	private readonly string folderName;
	private readonly string baseName;
	private readonly string ext;
	private readonly ScriptExtractionTransformerOpt opt;

	public ScriptExtractionTransformer(
		ReplierContentHolder contentHolder,
		Action<ScriptExtractionTransformerOpt>? optFun,
		Func<HtmlNode, bool> predicate,
		Func<string, string> makeDom,
		ReplyType replyType,
		string folderName,
		string baseName,
		string ext
	)
	{
		this.contentHolder = contentHolder;
		this.predicate = predicate;
		this.makeDom = makeDom;
		this.replyType = replyType;
		this.folderName = folderName;
		this.baseName = baseName;
		this.ext = ext;
		opt = ScriptExtractionTransformerOpt.Build(optFun);
		if (opt.GroupRestInLastName && opt.PredefinedNames.Count == 0) throw new ArgumentException();
	}

	public void Apply(HtmlNode root)
	{
		var headChildren = root.FindNodeByName("head").ChildNodes;
		var nodes = headChildren.WhereToArray(predicate);
		var maps = MapNodes(nodes);

		maps.ForEach(e =>
		{
			e.Nodes.Delete();
			var dom = makeDom(e.Link);
			var domNode = HtmlNode.CreateNode(dom);
			headChildren.Append(domNode);

			contentHolder.AddContent(e.Link, new ContentNfo(replyType, ContentType.String, e.Content));
		});
	}

	private record ScriptMapNfo(HtmlNode[] Nodes, string Link, string Content);

	private ScriptMapNfo[] MapNodes(HtmlNode[] nodes)
	{
		if (opt.GroupRestInLastName)
		{
			var firstsCount = Math.Min(opt.PredefinedNames.Count - 1, nodes.Length);
			var mapFirsts = nodes.Take(firstsCount)
				.SelectToArray((e, i) => new ScriptMapNfo(
					new[] { e },
					MkLink(i),
					e.InnerText
				));
			var others = nodes.Skip(firstsCount).ToArray();
			if (others.Length == 0)
			{
				return mapFirsts;
			}
			else
			{
				var cssLast = new ScriptMapNfo(
					others,
					MkLink(opt.PredefinedNames.Count - 1),
					string.Join(Environment.NewLine + Environment.NewLine, others.Select(e => e.InnerText))
				);
				return mapFirsts.Append(cssLast).ToArray();
			}
		}
		else
		{
			return nodes
				.SelectToArray((e, i) => new ScriptMapNfo(
					new[] { e },
					MkLink(i),
					e.InnerText
				));
		}
	}

	private string MkLink(int idx) =>
		(idx < opt.PredefinedNames.Count) switch
		{
			true => $"{folderName}/{opt.PredefinedNames[idx]}.{ext}",
			false => $"{folderName}/{baseName}_{idx - opt.PredefinedNames.Count}.{ext}"
		};
}


static class ScriptExtractionTransformerExt
{
	public static bool IsCssNode(this HtmlNode node) => node.Name == "style";
	
	public static bool IsJsNode(this HtmlNode node) => node.Name == "script";

	public static void Delete(this HtmlNode[] nodes)
	{
		foreach (var node in nodes)
			node.Remove();
	}
}