using System.Net;
using System.Text;
using LINQPadExtras.PageServing.Utils.Exts;

namespace LINQPadExtras.PageServing.Replying;

static class ReplierExt
{
	public static string GetUrl(this HttpListenerRequest req)
	{
		if (req.Url == null) throw new ArgumentException();
		return req.Url.AbsolutePath.RemoveLeadingSlash();
	}

	public static byte[] ToBytes(this string str) => Encoding.UTF8.GetBytes(str);
}