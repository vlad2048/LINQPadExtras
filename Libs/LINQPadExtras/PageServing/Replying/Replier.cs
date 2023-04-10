using System.Net;
using LINQPadExtras.PageServing.PageLogic;
using LINQPadExtras.PageServing.Replying.Structs;
using PowMaybe;

namespace LINQPadExtras.PageServing.Replying;

public record ReplierCtx(
	int HttpPort
);

public interface IReplier
{
	void AssignCtx(ReplierCtx ctx);
	bool CanHandleUrl(string url);
	Task Reply(HttpListenerRequest req, HttpListenerResponse res, int ctxId);
}

class Replier
{
	private readonly ReplierContentHolder contentHolder;
	private readonly PageMutator pageMutator;
	private readonly IReplier[] repliers;

    public Replier(
	    ReplierContentHolder contentHolder,
	    PageMutator pageMutator,
	    IReplier[] repliers
	)
    {
	    this.contentHolder = contentHolder;
	    this.pageMutator = pageMutator;
	    this.repliers = repliers;
    }

    public async Task Reply(HttpListenerRequest req, HttpListenerResponse res)
    {
	    var mayReply = await GetPageOrContentReply(req);
	    if (mayReply.IsSome(out var reply))
	    {
		    await reply.Write(res);
		    return;
	    }

	    var url = req.GetUrl();
        foreach (var replier in repliers)
        {
	        if (replier.CanHandleUrl(url))
	        {
		        await replier.Reply(req, res, 0);
	        }
        }
    }

    private async Task<Maybe<Reply>> GetPageOrContentReply(HttpListenerRequest req)
    {
	    var url = req.GetUrl();

	    if (url == string.Empty)
	    {
		    var pageHtml = pageMutator.GetPage();
		    return May.Some(new Reply(pageHtml.ToBytes(), ReplyType.Html));
	    }

	    if (contentHolder.TryGetContent(url, out var content))
	    {
		    return content.ContentType switch
		    {
			    ContentType.String => May.Some(new Reply(
				    content.Str.ToBytes(),
				    content.Type
			    )),
			    ContentType.File => May.Some(new Reply(
				    await File.ReadAllBytesAsync(content.Str),
				    content.Type
			    )),
			    _ => throw new ArgumentException(),
		    };
	    }

	    return May.None<Reply>();
    }
}
