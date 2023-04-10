using LINQPad.Controls;

namespace LINQPadExtras.PageServing.Utils.HtmlUtils;

static class HtmlIdUtils
{
	public static string GetId(this Control ctrl) => ctrl.HtmlElement.ID;

	public static string IncId(this string id)
	{
		var num = int.Parse(id[2..]);
		return $"{id[..2]}{num + 1}";
	}
}