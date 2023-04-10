using System.Net;
using System.Text;

namespace LINQPadExtras.PageServing.Replying.Structs;

public record Reply(
    byte[] Data,
    ReplyType Type
)
{
    public async Task Write(HttpListenerResponse response)
    {
        response.ContentType = Type switch
        {
            ReplyType.Html => "text/html",
            ReplyType.ScriptJs => "text/javascript",
            ReplyType.ScriptCss => "text/css",
            ReplyType.ImagePng => "image/png",
            ReplyType.ImageJpg => "image/jpeg",
            ReplyType.ImageSvg => "image/svg+xml",
			ReplyType.Video => "video/mp4",
            _ => throw new ArgumentException()
        };
        response.ContentEncoding = Type switch
        {
            ReplyType.Html => Encoding.UTF8,
            ReplyType.ScriptJs => Encoding.UTF8,
            ReplyType.ScriptCss => Encoding.UTF8,
            ReplyType.ImagePng => Encoding.Default,
            ReplyType.ImageJpg => Encoding.Default,
            ReplyType.ImageSvg => Encoding.UTF8,
            ReplyType.Video => Encoding.Default,
            _ => throw new ArgumentException()
        };
        response.ContentLength64 = Data.LongLength;
        await response.OutputStream.WriteAsync(Data, 0, Data.Length);
    }
}