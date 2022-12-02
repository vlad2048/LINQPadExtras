using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using LINQPad;
using LINQPadExtras.PageServing.Structs;
using LINQPadExtras.PageServing.Utils;
using LINQPadExtras.PageServing.Utils.Exts;
using PowMaybe;
using PowRxVar;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LINQPadExtras.PageServing.Components;

record WSCloseNfo(
	ushort Code,
	bool WasClean,
	string Reason
);

record WSErrorNfo(
	string Message,
	Exception Ex
);

record WSState(
	WebSocketState Status,
	Maybe<WSCloseNfo> CloseNfo,
	Maybe<WSErrorNfo> ErrorNfo
);

class Server : IDisposable
{
	private readonly Disp d = new();
	public void Dispose() => d.Dispose();

	private readonly int httpPort;
	private readonly HttpListener listener;
	private readonly WebSocketServer wsServer;
	private readonly ImageFixer imageFixer;
	private readonly ScriptTracker scriptTracker;
	private readonly PageBuilder pageBuilder;
	private readonly ISubject<Chg> whenChg;
	private RefreshBehavior? refresh;
	private bool isStarted;

	public IRoVar<WSState> WSState { get; }
	public IObservable<Chg> WhenChg => whenChg.AsObservable();
	public void SendData(string data) => refresh?.SendData(data);


	public Server(ImageFixer imageFixer, ScriptTracker scriptTracker, PageBuilder pageBuilder, int httpPort, int wsPort)
	{
		this.httpPort = httpPort;
		this.imageFixer = imageFixer;
		this.scriptTracker = scriptTracker;
		this.pageBuilder = pageBuilder;
		Disposable.Create(Stop).D(d);
		var wsStateVar = Var.Make(Var.Make(new WSState(
			WebSocketState.Closed,
			May.None<WSCloseNfo>(),
			May.None<WSErrorNfo>()
		)).ToReadOnly()).D(d);
		WSState = wsStateVar.SwitchVar(e => e);
		whenChg = new Subject<Chg>().D(d);
		wsServer = new WebSocketServer(wsPort);
		//wsServer.Log.Level = LogLevel.Trace;
		wsServer.Log.Level = LogLevel.Fatal + 1;
		listener = new HttpListener().D(d);

		var prefix = $"http://*:{httpPort}/";
		listener.Prefixes.Add(prefix);

		refresh = null!;
		wsServer.AddWebSocketService<RefreshBehavior>("/refresh", _refresh => {
			refresh = _refresh;
			wsStateVar.V = refresh.WSState;
			refresh.WhenChg.Subscribe(chg => whenChg.OnNext(chg)).D(d);
			refresh.WhenClose.Subscribe(_ =>
			{
				//"[WS Restart in a few secs]".Dump();
				Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ =>
				{
					//wsServer.Stop();
					//wsServer.Start();
				}).D(d);
			}).D(d);
		});
	}
	
	public void Start()
	{
		if (isStarted) return;
		isStarted = true;

		try
		{
			listener.Start();
		}
		catch (HttpListenerException ex) when (ex.ErrorCode == 5)
		{
			PrintAccessIssueHelp(httpPort);
			throw;
		}

		Task.Run(async () =>
		{
			while (isStarted)
			{
				HttpListenerContext ctx;
				try
				{
					ctx = await listener.GetContextAsync();
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				var req = ctx.Request;
				var resp = ctx.Response;
				if (req.Url == null) throw new ArgumentException();
				var url = req.Url.AbsolutePath;
				var urlRel = url.RemoveLeadingSlash();
				//$"[req]: '{url}'".Dump();
				if (url == "/")
				{
					var page = pageBuilder.BuildWholePage();
					var data = Encoding.UTF8.GetBytes(page);
					resp.ContentType = "text/html";
					resp.ContentEncoding = Encoding.UTF8;
					resp.ContentLength64 = data.LongLength;
					await resp.OutputStream.WriteAsync(data, 0, data.Length);
				}
				else if (ImageFixer.IsImageUrl(urlRel))
				{
					var imageFile = imageFixer.GetFileFromUrl(urlRel);
					var bytes = await File.ReadAllBytesAsync(imageFile);
					resp.ContentType = GetImageContentType(imageFile);
					resp.ContentLength64 = bytes.Length;
					await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
				}
				else if (scriptTracker.CanRespond(urlRel))
				{
					await scriptTracker.Respond(urlRel, resp);
				}
				resp.Close();
			}
		});
		wsServer.Start();
	}
	
	public void Stop()
	{
		if (!isStarted) return;
		isStarted = false;
		listener.Stop();
		wsServer.Stop();
	}

	private static readonly string[] filesToDelete =
	{
		"linqpad.css",
		"extra.css",
	};

	public void EditHtml(string folder)
	{
		if (string.IsNullOrEmpty(folder)) return;
		folder.CreateFolderIFN();

		foreach (var fileToDelete in filesToDelete)
		{
			var fullFilename = Path.Combine(folder, fileToDelete);
			if (File.Exists(fullFilename))
				File.Delete(fullFilename);
		}
		var page = pageBuilder.BuildWholePage();
		var pageFile = Path.Combine(folder, "index.html");
		File.WriteAllText(pageFile, page);
		scriptTracker.WriteAllToFolder(folder);
		imageFixer.WriteAllToFolder(folder);
	}

	private static string GetImageContentType(string file) =>
		Path.GetExtension(file) switch
		{
			".png" => "image/png",
			".jpg" => "image/jpeg",
			".jpeg" => "image/jpeg",
			_ => throw new ArgumentException($"Unknown image type: '{file}'")
		};


	private class RefreshBehavior : WebSocketBehavior, IDisposable
	{
		private readonly Disp d = new();
		public void Dispose() => d.Dispose();

		private readonly IRwVar<WSState> wsState;
		private readonly ISubject<Chg> whenChg;
		private readonly ISubject<Unit> whenClose;

		public IRoVar<WSState> WSState => wsState.ToReadOnly();
		public IObservable<Chg> WhenChg => whenChg.AsObservable();
		public IObservable<Unit> WhenClose => whenClose.AsObservable();
		public void SendData(string data)
		{
			if (State != WebSocketState.Open) return;
			Send(data);
		}

		public RefreshBehavior()
		{
			wsState = Var.Make(new WSState(
				State,
				May.None<WSCloseNfo>(),
				May.None<WSErrorNfo>()
			)).D(d);
			whenClose = new Subject<Unit>().D(d);
			whenChg = new Subject<Chg>().D(d);
		}

		protected override void OnOpen()
		{
			wsState.V = new WSState(
				State,
				May.None<WSCloseNfo>(),
				May.None<WSErrorNfo>()
			);
			//"[WS-Open-0]".Dump();
			base.OnOpen();
			//"[WS-Open-1]".Dump();
		}

		protected override void OnClose(CloseEventArgs e)
		{
			wsState.V = wsState.V with
			{
				Status = State,
				CloseNfo = May.Some(new WSCloseNfo(e.Code, e.WasClean, e.Reason))
			};
			//"[WS-Close-0]".Dump();
			base.OnClose(e);
			//"[WS-Close-1]".Dump();
			whenClose.OnNext(Unit.Default);
		}

		protected override void OnError(WebSocketSharp.ErrorEventArgs e)
		{
			wsState.V = wsState.V with
			{
				Status = State,
				ErrorNfo = May.Some(new WSErrorNfo(e.Message, e.Exception))
			};
			//$"[error] {e.Message}".Dump();
			//e.Exception.Dump();
			//"[WS-Error-0]".Dump();
			base.OnError(e);
			//"[WS-Error-1]".Dump();
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			var str = e.Data;
			var chg = str.Deser<Chg>();
			whenChg.OnNext(chg);
			//"[WS-Message-0]".Dump();
			base.OnMessage(e);
			//"[WS-Message-1]".Dump();
		}
	}


	private static void PrintAccessIssueHelp(int httpPort) => Util.FixedFont(GetAccessIssueHelp(httpPort)).Dump();

	private static string GetAccessIssueHelp(int httpPort) => $"""
	
		"Access is denied. (error code 5)" error was thrown when calling HttpListener.Start()

		Make sure you run this command in admin mode:
			netsh http add urlacl url=http://*:6240/ user=Everyone 
			(alternatively you can run linqpad in admin mode and you don't need to mess with urlacls)
		
		To delete the urlacl rule:
			netsh http delete urlacl url=http://*:6240/
		
		To view the urlacl rule:
			netsh http show urlacl url=http://*:6240/
				
		Additionally to access the page from another computer, open the port in the firewall:
			netsh advfirewall firewall add rule name="__linqpad_webserver_http" dir=in action=allow protocol=TCP localport={httpPort} profile=public 

		To close the port in the firewall:
			netsh advfirewall firewall delete rule name="__linqpad_webserver_http"
		
		""";
}
