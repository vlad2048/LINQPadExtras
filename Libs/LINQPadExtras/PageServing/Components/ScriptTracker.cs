using System.Net;
using System.Text;
using HtmlAgilityPack;
using LINQPadExtras.PageServing.Utils.Exts;

namespace LINQPadExtras.PageServing.Components;

enum ScriptType
{
	Css,
	Js
}

class ScriptTracker
{
	private record ScriptNfo(string Link, ScriptType Type, string Content)
	{
		//public string Name => Path.GetFileNameWithoutExtension(Link);
		public string Filename => Link.Link2Path();
	}

	private readonly List<ScriptNfo> scripts = new();


	public string TweakHead(string headHtml)
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(headHtml);
		var root = doc.DocumentNode;

		var nodes = root.ChildNodes.Where(e => e.IsCssNode() || e.IsJsNode()).ToArray();
		var links = nodes.GetLinks();
		for (var i = 0; i < nodes.Length; i++)
		{
			var node = nodes[i];
			var link = links[i];
			var content = node.InnerText;
			var type = (node.IsCssNode(), node.IsJsNode()) switch
			{
				(true, false) => ScriptType.Css,
				(false, true) => ScriptType.Js,
				_ => throw new ArgumentException()
			};
			if (i < scripts.Count)
			{
				var script = scripts[i];
				if (type != script.Type) throw new ArgumentException();
				scripts[i] = scripts[i] with { Content = content };
			}
			else
			{
				var script = new ScriptNfo(link, type, content);
				scripts.Add(script);
			}

			node.Remove();
		}

		foreach (var script in scripts)
		{
			var nodeHtml = script.Type switch
			{
				ScriptType.Css => $"""<link href="{script.Link}" rel="stylesheet"/>""" ,
				ScriptType.Js => $"""<link href="{script.Link}"/>""" ,
				_ => throw new ArgumentException()
			};
			var nodeLink = HtmlNode.CreateNode(nodeHtml);
			root.ChildNodes.Append(nodeLink);
		}

		return root.OuterHtml;
	}


	public bool CanRespond(string url) => scripts.Any(e => e.Link == url);

	public async Task Respond(string url, HttpListenerResponse resp)
	{
		var nfo = scripts.Single(e => e.Link == url);
		var data = Encoding.UTF8.GetBytes(nfo.Content);
		resp.ContentType = nfo.Type switch
		{
			ScriptType.Css => "text/css",
			ScriptType.Js => "text/javascript",
			_ => throw new ArgumentException()
		};
		resp.ContentEncoding = Encoding.UTF8;
		resp.ContentLength64 = data.LongLength;
		await resp.OutputStream.WriteAsync(data, 0, data.Length);
	}

	public void WriteAllToFolder(string rootFolder)
	{
		foreach (var nfo in scripts)
		{
			var file = Path.Combine(rootFolder, nfo.Filename).CreateFolderForFileIFN();
			File.WriteAllText(file, nfo.Content);
		}
	}

}

static class ScriptTrackerExt
{
	public static string GetFolderName(this ScriptType type) => type switch
	{
		ScriptType.Css => "css",
		ScriptType.Js => "js",
		_ => throw new ArgumentException()
	};

	public static bool IsCssNode(this HtmlNode node) => node.Name == "style";
	public static bool IsJsNode(this HtmlNode node) => node.Name == "script";

	public static string[] GetLinks(this HtmlNode[] nodes)
	{
		var cssIdx = -1;
		var jsIdx = -1;
		var list = new List<string>();
		foreach (var node in nodes)
		{
			if (node.IsCssNode())
			{
				var name = (cssIdx == -1) switch
				{
					true => "linqpad",
					false => $"style_{cssIdx}"
				};
				var link = $"{ScriptType.Css.GetFolderName()}/{name}.css";
				list.Add(link);
				cssIdx++;
			}
			else if (node.IsJsNode())
			{
				var name = (jsIdx == -1) switch
				{
					true => "linqpad",
					false => $"script_{jsIdx}"
				};
				var link = $"{ScriptType.Js.GetFolderName()}/{name}.js";
				list.Add(link);
				jsIdx++;
			}
			else throw new ArgumentException();
		}
		return list.ToArray();
	}
}