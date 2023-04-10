namespace LINQPadExtras.PageServing;

static class FrontendScripts
{
	public const string RefreshStatusId = "refresh-status";


	public const string MainDivId = "final";

	public const string JSNotifyLogic = $$"""
		<script>
			updateState();
		</script>
		""";

	public static string MachineName => Environment.MachineName.ToLowerInvariant();

	public static string JSRefreshLogic(int wsPort) => $$"""
		const socketUrl = "ws://{{MachineName}}:{{wsPort}}/refresh";
		let closeCode = -1;
		let closeWasClean = false;
		let closeReason = '';
		let socket = null;

		function updateState() {
			const elt = document.getElementById('{{RefreshStatusId}}');
			switch (socket.readyState) {
				case WebSocket.CONNECTING: elt.innerText = 'CONNECTING'; break;
				case WebSocket.OPEN: elt.innerText = 'OPEN'; break;
				case WebSocket.CLOSING: elt.innerText = 'CLOSING'; break;
				case WebSocket.CLOSED: elt.innerText = `CLOSED code:${closeCode} wasClean:${closeWasClean} reason:${closeReason}`; break;
				default: elt.innerText = `Unknown:${socket.readyState}`; break;
			}
		}

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

				const eltBody = document.getElementById('{{MainDivId}}');
				const eltHead = document.getElementsByTagName('head')[0];

				if (document.querySelector('html').style.cssText !== data.htmlStyle) {
					var val = data.htmlStyle;
					if (val === '')
						val = undefined;
					document.querySelector('html').style.cssText = val;
				}

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
		""";
}