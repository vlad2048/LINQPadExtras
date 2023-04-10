using HtmlAgilityPack;

namespace LINQPadExtras.PageServing.Utils.HtmlUtils;

static class HtmlNodeUtils
{
	public static void ForEachNode(this HtmlNode root, Action<HtmlNode> action)
	{
		void Recurse(HtmlNode n)
		{
			action(n);
			foreach (var childN in n.ChildNodes)
				Recurse(childN);
		}
		Recurse(root);
	}

	public static void ForEachNode(this HtmlDocument doc, Action<HtmlNode> action)
	{
		void Recurse(HtmlNode n)
		{
			action(n);
			foreach (var childN in n.ChildNodes)
				Recurse(childN);
		}
		Recurse(doc.DocumentNode);
	}

	public static HtmlNode FindNodeByName(this HtmlNode root, string name) =>
		root.FindNodes(e => e.Name == name)[0];

	public static HtmlNode FindNodeById(this HtmlNode root, string id) =>
		root.FindNodes(e => e.Id == id)[0];

	public static HtmlNode[] FindNodes(this HtmlNode root, Func<HtmlNode, bool> predicate)
	{
		var list = new List<HtmlNode>();
		void Recurse(HtmlNode n)
		{
			if (predicate(n)) list.Add(n);
			foreach (var childN in n.ChildNodes)
				Recurse(childN);
		}
		Recurse(root);
		return list.ToArray();
	}

	public static string? GetAttrOpt(this HtmlNode node, string attrName)
	{
		var attr = node.Attributes.FirstOrDefault(e => e.Name == attrName);
		if (attr == null) return null;
		return attr.Value;
	}

	public static void SetAttr(this HtmlNode node, string attrName, string attrVal)
	{
		node.RemoveAttrIFN(attrName);
		var attr = node.OwnerDocument.CreateAttribute(attrName, attrVal);
		node.Attributes.Add(attr);
	}
	
	public static void SetChecked(this HtmlNode node, bool set)
	{
		node.RemoveAttrIFN("checked");
		if (set)
		{
			var attr = node.OwnerDocument.CreateAttribute("checked", "true");
			node.Attributes.Add(attr);
		}
	}

	public static void SetOnClick(this HtmlNode node, string code)
	{
		node.RemoveAttrIFN("onclick");
		var attr = node.OwnerDocument.CreateAttribute("onclick", code);
		node.Attributes.Add(attr);
	}

	private static void RemoveAttrIFN(this HtmlNode node, string attrName)
	{
		var attr = node.Attributes.FirstOrDefault(e => e.Name == attrName);
		if (attr != null)
			node.Attributes.Remove(attr);
	}
}