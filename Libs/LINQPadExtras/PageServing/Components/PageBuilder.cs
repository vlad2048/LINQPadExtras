using HtmlAgilityPack;
using LINQPad;
using LINQPadExtras.PageServing.Utils.HtmlUtils;

namespace LINQPadExtras.PageServing.Components;

/*class Syncers
{
	private readonly Dictionary<string, ISyncer> syncers = new();
	private readonly List<ITweaker> tweakers = new();
	public ISyncer GetSyncer(string type) => syncers[type];
	public ITweaker[] GetTweakers() => tweakers.ToArray();
	public void AddSyncers(params ISyncer[] pSyncers)
	{
		foreach (var syncer in pSyncers)
		{
			syncers[syncer.Type] = syncer;
			tweakers.Add(syncer);
		}
	}
	public void AddTweaker(ITweaker tweaker) => tweakers.Add(tweaker);
}*/

class Tweaks
{
	private readonly Dictionary<string, Action<HtmlNode>> tweakMap = new();

	public void SetTweak(string id, Action<HtmlNode> action) => tweakMap[id] = action;

	public Action<HtmlNode>? GetTweakOpt(string id) => tweakMap.TryGetValue(id, out var action) switch
	{
		true => action,
		false => null
	};
}

class PageBuilder
{
	private const string RefreshStatusId = "refresh-status";
	private const string MainDivId = "main-div";
	private readonly ImageFixer imageFixer;
	private readonly ScriptTracker scriptTracker;
	private readonly Tweaks tweaks;
	private readonly int wsPort;

	public const string ServerConnectionId = "server-connection";
	public static string MachineName => Environment.MachineName.ToLowerInvariant();

	public PageBuilder(ImageFixer imageFixer, ScriptTracker scriptTracker, Tweaks tweaks, int wsPort)
	{
		this.imageFixer = imageFixer;
		this.scriptTracker = scriptTracker;
		this.tweaks = tweaks;
		this.wsPort = wsPort;
	}

	public static string GetHtmlStyle() => (string)Util.InvokeScript(true, "eval", "document.querySelector('html').style.cssText");

	public string GetTweakedHead()
	{
		var head = HtmlPageReadingUtils.GetHead();
		var tweakedHead = scriptTracker.TweakHead(head);
		return tweakedHead;
	}

	public string GetTweakedBody()
	{
		var bodyHtml = HtmlPageReadingUtils.GetBody();
		var doc = new HtmlDocument();
		doc.LoadHtml(bodyHtml);
		HtmlNode? nodeToRemove = null;
		doc.ForEachNode(node =>
		{
			if (node.Id == ServerConnectionId)
			{
				nodeToRemove = node;
				return;
			}
			var tweakAction = tweaks.GetTweakOpt(node.Id);
			tweakAction?.Invoke(node);
		});
		nodeToRemove?.Remove();
		imageFixer.TweakBody(doc);
		return doc.DocumentNode.OuterHtml;
	}
	
	public string BuildWholePage()
	{
		var head = GetTweakedHead();
		var body = GetTweakedBody();
		var html = $"""
			<!DOCTYPE html>
			<html>
				<head>
					{head}
					{JSRefreshLogic(wsPort)}
				</head>
				<body>
					{HtmlRefreshUI}
					<div id="{MainDivId}">
						{body}
					</div>
					{JSNotifyLogic}
				</body>
			</html>
			""";
		return html.Beautify(false);
	}

	private const string JSHookInputs = """
			function hookInputs() {
				const nodes = document.querySelectorAll("input[type='checkbox']");
				for (let node of nodes) {
					console.log(`node:${node.id}`);
					const id = node.id;
					node.addEventListener('change', evt => {
						if (socket.readyState !== WebSocket.OPEN) { console.log('Change but socket not OPEN'); return; }
						const val = node.checked.toString();
						//console.log(`checkbox[${id}]<-${val}`);
						const chg = { type: 'checkbox', id, val };
						const str = JSON.stringify(chg);
						//console.log(str);
						socket.send(str);
					});
				}
			}
""";

	private const string JSNotifyLogic = $$"""
		<script>
			updateState();
			//hookInputs();
		</script>
	""";

	private const string HtmlRefreshUI = $"""
		<div class="connection">
			<span>Client connection:</span>
			<span id="{RefreshStatusId}">not init</span>
		</div>
		""";

	private static string JSRefreshLogic(int wsPort) => $$"""
		<script>
			const socketUrl = "ws://{{MachineName}}:{{wsPort}}/refresh";
			let closeCode = -1;
			let closeWasClean = false;
			let closeReason = '';
			let socket = null;

			function updateState() {
				const elt = document.getElementById('refresh-status');
				switch (socket.readyState) {
					case WebSocket.CONNECTING: elt.innerText = 'CONNECTING'; break;
					case WebSocket.OPEN: elt.innerText = 'OPEN'; break;
					case WebSocket.CLOSING: elt.innerText = 'CLOSING'; break;
					case WebSocket.CLOSED: elt.innerText = `CLOSED code:${closeCode} wasClean:${closeWasClean} reason:${closeReason}`; break;
					default: elt.innerText = `Unknown:${socket.readyState}`; break;
				}
			}

			// {JSHookInputs}

			function connectSocket() {
				socket = new WebSocket(socketUrl);

				socket.onopen = e => {
					updateState();
				}
				socket.onclose = e => {
					closeCode = e.code;
					closeWasClean = e.wasClean;
					closeReason = e.reason;
					updateState();
					console.log(`[close] code:${e.code} clean:${e.wasClean} reason:${e.reason}`);
					setTimeout(() => {
					connectSocket();
					}, 1000);
				}
				socket.onerror = e => {
					updateState();
					console.log('[error]');
					console.log(e);
				}

				socket.onmessage = e => {
					const data = JSON.parse(e.data);

					const eltBody = document.getElementById('main-div');
					const eltHead = document.getElementsByTagName('head')[0];

					if (document.querySelector('html').style.cssText !== data.htmlStyle)
						document.querySelector('html').style.cssText = data.htmlStyle;

					if (eltBody.innerHTML !== data.body)
						eltBody.innerHTML = data.body;

					if (eltHead.innerHTML !== data.head)
						eltHead.innerHTML = data.head;
				}
			}

			connectSocket();



			function send(type, id, val = null) {
				if (socket.readyState !== WebSocket.OPEN) return;
				const str = JSON.stringify({ type, id, val });
				socket.send(str);
			}


		</script>
""";
}