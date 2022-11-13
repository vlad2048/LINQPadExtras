namespace LINQPadExtras.Utils.Exts;

static class StringExt
{
	public static string QuoteIFN(this string s) => s.Contains(" ") switch
	{
		true => $@"""{s}""",
		false => s
	};
}