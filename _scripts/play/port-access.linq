<Query Kind="Program">
  <NuGetReference>WebSocketSharp-netstandard</NuGetReference>
  <Namespace>System.Net</Namespace>
  <Namespace>WebSocketSharp.Server</Namespace>
  <Namespace>WebSocketSharp</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

using Acc = AccessUtils;

void Main()
{
	var httpPort = 6240;
	var wsPort = 9730;

	Acc.FireInDel(httpPort);
	Acc.FireInDel(wsPort);
	Acc.FireOutDel(httpPort);
	Acc.FireOutDel(wsPort);
	Acc.AclDel(httpPort);
	Acc.AclDel(wsPort);
	
	Acc.FireInAdd(httpPort);	// needed even in Admin mode
	Acc.AclAdd(httpPort);		// needed in non Admin mode
	
	//Acc.FireOutAdd(httpPort);

	//Acc.AclAdd(6240);
	//Acc.AclDel(6240);
	//Acc.FireInAdd(6240);
	//Acc.FireInDel(6240);
	
	//Acc.FireInAdd(6240);
	//Acc.AclAdd(6240);
	//Acc.FireInAdd(9730);
	//Acc.AclAdd(9730);


	Acc.AclGetRules().Reverse().Dump();

	//TestPorts(httpPort, null);
	
	//Acc.AclAdd
}

public static void TestPorts(int httpPort, int? wsPort)
{
	var listener = new HttpListener();
	var prefix = $"http://{Environment.MachineName.ToLowerInvariant()}:{httpPort}/";
	listener.Prefixes.Add(prefix);
	$"Http listening on: '{prefix}'".Dump();

	WebSocketServer? wsServer = null;
	if (wsPort.HasValue)
	{
		wsServer = new WebSocketServer(wsPort.Value);
		wsServer.AddWebSocketService<RefreshBehavior>("/refresh");
	}
	
	listener.Start();
	var isStarted = true;
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
			//$"[req]: '{url}'".Dump();
			if (url == "/")
			{
				var page = "";
				var data = Encoding.UTF8.GetBytes(page);
				resp.ContentType = "text/html";
				resp.ContentEncoding = Encoding.UTF8;
				resp.ContentLength64 = data.LongLength;
				await resp.OutputStream.WriteAsync(data, 0, data.Length);
			}
			resp.Close();
		}
	});
	wsServer?.Start();
}

class RefreshBehavior : WebSocketBehavior
{
	protected override void OnOpen()
	{
		"[WS] [open]".Dump();
		base.OnOpen();
	}
	protected override void OnClose(CloseEventArgs e)
	{
		$"[WS] [close] code:{e.Code} reason:{e.Reason} clean:{e.WasClean}".Dump();
		base.OnClose(e);
	}

	protected override void OnError(WebSocketSharp.ErrorEventArgs e)
	{
		$"[WS] [error] {e.Message}".Dump();
		e.Exception.Dump();
		base.OnError(e);
	}

	protected override void OnMessage(MessageEventArgs e)
	{
		$"[WS] [msg] {e.Data}".Dump();
		base.OnMessage(e);
	}
}


public enum FirewallDirection { In, Out };

static class AccessUtils
{
	/*
		netsh advfirewall firewall add rule name="__linqpad_webserver_http" dir=in action=allow protocol=TCP localport=6240 profile=public
		netsh advfirewall firewall delete rule name="__linqpad_webserver_http"

		netsh http add urlacl url=http://*:6240/ user=Everyone
		netsh http show urlacl url=http://*:6240/
		netsh http delete urlacl url=http://*:6240/
	*/

	private static string MkFireName(int port, FirewallDirection dir) => $"__linqpad_{dir.ToString().ToLowerInvariant()}_{port}";

	public static void AclAdd(int port) => UrlaclAdd(port);
	public static void AclDel(int port) => UrlaclRemove(port);
	public static void FireInAdd(int port) => FirewallOpen(port, FirewallDirection.In, MkFireName(port, FirewallDirection.In));
	public static void FireInDel(int port) => FirewallClose(MkFireName(port, FirewallDirection.In));
	public static void FireOutAdd(int port) => FirewallOpen(port, FirewallDirection.Out, MkFireName(port, FirewallDirection.Out));
	public static void FireOutDel(int port) => FirewallClose(MkFireName(port, FirewallDirection.Out));


	private static void UrlaclAdd(int port) => Util.Cmd("netsh", $"http add urlacl url=http://*:{port}/ user=Everyone", quiet: true);
		
	private static void UrlaclRemove(int port) => Util.Cmd("netsh", $"http delete urlacl url=http://*:{port}/", quiet: true);
	
	private static void FirewallOpen(int port, FirewallDirection dir, string ruleName)
	{
		var prof = FirewallProfile.Public;
		if (FirewallIsPortOpen(port, dir, prof)) return;
		var lines = Util.Cmd("netsh", $"""advfirewall firewall add rule name="{ruleName}" dir={dir.ToString().ToLowerInvariant()} action=allow protocol=TCP localport={port} profile={prof.ToString().ToLowerInvariant()}""", quiet: true).ToArray();
	}
	
	private static void FirewallClose(string ruleName)
	{
		Util.Cmd("netsh", $"""advfirewall firewall delete rule name="{ruleName}" """, quiet: true);
	}



	private static bool FirewallIsPortOpen(int port, FirewallDirection direction, FirewallProfile profile) =>
		Util.Cmd("netsh", $"advfirewall firewall show rule name=all dir={direction.ToString().ToLowerInvariant()} type=static", quiet: true)
			.ParseFirewallRules()
			.Any(rule =>
				rule.Enabled &&
				(rule.Protocol == "TCP" || rule.Protocol == "Any") &&
				DoesPortContain(rule.LocalPort, port) &&
				rule.Profile.HasFlag(profile)
			);
			
	
	public static AclRule[] AclGetRules() =>
		Util.Cmd("netsh", "http show urlacl", quiet: true)
			.ParseAclRules()
			.ToArray();
	
	public record AclRule(
		string Url
	);
	
	private static AclRule[] ParseAclRules(this IEnumerable<string> linesSource)
	{
		var lines = linesSource.ToArray();
		return lines
			.Where(e => e.Trim().StartsWith("Reserved URL"))
			.Select(e => new AclRule(e.ReadAclUrl()))
			.ToArray();
	}
	
	private static string ReadAclUrl(this string s)
	{
		var idx = s.IndexOf(':');
		return s[(idx + 1)..].Trim();
	}
	
	
	
	

	[Flags] private enum FirewallProfile { Private = 1, Domain = 2, Public = 4 };
	[Flags] private enum FirewallProtocol { Tcp = 1, Udp = 2 }; //, ICMPv4 = 4, ICMPv6 = 8 };
	
	private record FirewallRule(
		bool Enabled,
		string Name,
		FirewallDirection Direction,
		FirewallProfile Profile,
		//FirewallProtocol Protocol,
		string Protocol,
		string LocalIP,
		string RemoteIP,
		string LocalPort,
		string RemotePort,
		string Action
	);

	private static FirewallRule[] ParseFirewallRules(this IEnumerable<string> linesSource)
	{
		var lines = linesSource.ToArray();
		return lines
			.GetLineIndices(e => e.StartsWith("-----"))
			.Select(i => lines.Skip(i - 1).TakeWhile(e => e.Trim() != string.Empty).ToArray())
			//.Where(rs => rs.Read("Protocol").IsProtocolValid())
			.Select(rs => new FirewallRule(
				rs.Read("Enabled").ToBool(),
				rs.Read("Rule Name"),
				rs.Read("Direction").ToDirection(),
				rs.Read("Profiles").ToProfiles(),
				rs.Read("Protocol"), //.ToProtocol(),
				rs.Read("LocalIP"),
				rs.Read("RemoteIP"),
				rs.ReadOpt("LocalPort"),
				rs.ReadOpt("RemotePort"),
				rs.Read("Action")
			))
			.ToArray();
	}
	
	private static bool DoesPortContain(string s, int port)
	{
		if (s == string.Empty || s.Any(char.IsAsciiLetter)) return false;
		var parts = s.Split(',');
		return parts.Any(part => DoesPortRangeContain(part, port));
	}
	private static bool DoesPortRangeContain(string s, int port)
	{
		if (s.Contains('-'))
		{
			var parts = s.Split('-');
			var portMin = int.Parse(parts[0]);
			var portMax = int.Parse(parts[1]);
			return port >= portMin && port <= portMax;
		}
		else
		{
			var portVal = int.Parse(s);
			return port == portVal;
		}
	}
	
	private static bool IsProtocolValid(this string s) => s == "TCP" || s == "UDP" || s == "Any";
	
	private static FirewallRule[] ParseFirewallRulesOld(string[] lines)
	{
		var indices = GetLineIndices(lines, e => e.StartsWith("-----"));
		var idx = indices[5];
		
		var rs = lines.Skip(idx - 1).Take(13).ToArray();
		rs.Dump();
		
		return Array.Empty<FirewallRule>();
	}

	private static string Read(this string[] ruleLines, string header) => ruleLines.Single(e => e.StartsWith($"{header}:")).ExtractValue();
	private static string ReadOpt(this string[] ruleLines, string header)
	{
		var line = ruleLines.FirstOrDefault(e => e.StartsWith($"{header}:"));
		if (line == null) return string.Empty;
		return line.ExtractValue();
	}
	private static bool ToBool(this string s) => s switch
	{
		"Yes" => true,
		"No" => false,
		_ => throw new ArgumentException()
	};
	private static FirewallDirection ToDirection(this string s) => s switch
	{
		"In" => FirewallDirection.In,
		"Out" => FirewallDirection.Out,
		_ => throw new ArgumentException()
	};
	private static FirewallProtocol ToProtocol(this string s) => s switch
	{
		"TCP" => FirewallProtocol.Tcp,
		"UDP" => FirewallProtocol.Udp,
		"Any" => FirewallProtocol.Tcp | FirewallProtocol.Udp,
		//"ICMPv6" => FirewallProtocol.ICMPv6,
		//"ICMPv4" => FirewallProtocol.ICMPv4,
		_ => throw new ArgumentException()
	};
	private static FirewallProfile ToProfiles(this string s)
	{
		var parts = s.Split(',');
		FirewallProfile val = 0;
		foreach (var part in parts)
		{
			var partVal = part switch
			{
				"Private" => FirewallProfile.Private,
				"Domain" => FirewallProfile.Domain,
				"Public" => FirewallProfile.Public,
				_ => throw new ArgumentException()
			};
			val |= partVal;
		}
		return val;
	}
	/*private static int[] ToPort(this string s)
	{
		if (s == "Any") return Array.Empty<int>();
		return s.Split(
	}*/
	
	private static string ExtractValue(this string line)
	{
		var idx = line.IndexOf(':');
		var str = line[(idx + 1)..].Trim();
		return str;
	}
	
	private static int[] GetLineIndices(this string[] lines, Func<string, bool> predicate)
	{
		var list = new List<int>();
		for (var i = 0; i < lines.Length; i++)
			if (predicate(lines[i]))
				list.Add(i);
		return list.ToArray();
	}
}