<Query Kind="Program">
  <Reference>C:\Dev\WebExtractor\LibsBase\ApiStructsLib\bin\Debug\net7.0\ApiStructsLib.dll</Reference>
  <Reference>C:\Dev\WebExtractor\Libs\CreepyLib\bin\Debug\net7.0-windows\CreepyLib.dll</Reference>
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0-windows\LINQPadExtras.dll</Reference>
  <Namespace>CreepyLib._CtxHolder.DataHolding</Namespace>
  <Namespace>ApiStructsLib.Api.BatchCreate</Namespace>
  <Namespace>ApiStructsLib.Api.BatchRun</Namespace>
  <Namespace>ApiStructsLib.Core</Namespace>
  <Namespace>ApiStructsLib.Api.BatchCheck</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>PowBasics.Geom</Namespace>
  <Namespace>RestSharp</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

public const string BaseUrl = "http://localhost:5200";


void Main()
{
	ApiBatchCheck("https://missav.com/en/release?page=1").Dump();
}

void Create(string url, string name)
{
	var res = ApiBatchCheck(url);
	res.Dump();
	if (res.AllFound)
	{
		"All found".Dump();
		ApiBatchCreate(new ApiBatchCreateParam(name, res.CreateNfo));
	}
	else
	{
		"Error".Dump();
	}
}

void Complete(string url, int? startIndex)
{
	//ApiBatchCheck(url).Dump();
	ApiBatchMakeReady(url);
	ApiBatchRun(new ApiBatchRunParam(url, startIndex, null));
}



void CheckAndCreate(string batchUrl, string batchName)
{
	var check = ApiBatchCheck(batchUrl);
	check.Dump();
	Util.ReadLine("Press a key to create");
	var createNfo = new ApiBatchCreateParam(batchName, check.CreateNfo);
	ApiBatchCreate(createNfo);
}


Batch[]					ApiBatchList()												=> Http.Get ("/api/batch/list").ReadJson<Batch[]>();
ApiBatchCheckResult		ApiBatchCheck(string batchUrl)								=> Http.Get ("/api/batch/check", ("batchUrl", batchUrl)).ReadJson<ApiBatchCheckResult>();
void					ApiBatchCreate(ApiBatchCreateParam param)					=> Http.Post("/api/batch/create", param);
void					ApiBatchMakeReady(string batchUrl)							=> Http.Get ("/api/batch/makeReady", ("batchUrl", batchUrl));
void					ApiBatchRun(ApiBatchRunParam param)							=> Http.Post("/api/batch/run", param);
VideoB[]				ApiBatchLoadVideos(string[] batchUrls)						=> Http.Get ("/api/batch/loadvideos", ("batchUrls", batchUrls.JoinText(","))).ReadJson<VideoB[]>();
void					ApiVideoGetLinks(string videoPageUrl, string? batchUrl)		=> Http.Get ("/api/video/getlinks", ("videoPageUrl", videoPageUrl), ("batchUrl", batchUrl)).ReadJson<string[]>();




public static class Http
{
	public static RestResponse Post(string url, object body)
	{
		var client = new RestClient(MkUrl(url));
		var req = new RestRequest
		{
			Method = Method.Post
		};
		req.AddJsonBody(body);

		var res = client.ExecuteAsync(req).Result;
		if (!res.IsSuccessful) throw new InvalidOperationException($"Query not successfull StatusCode={res.StatusCode}");
		return res;
	}
	
	public static RestResponse Get(string url, params (string, string?)[] queryParams)
	{
		var client = new RestClient(MkUrl(url));
		var req = new RestRequest();
		foreach (var t in queryParams) if (t.Item2 != null) req.AddQueryParameter(t.Item1, t.Item2);

		var res = client.ExecuteAsync(req).Result;
		if (!res.IsSuccessful) throw new InvalidOperationException($"Query not successfull StatusCode={res.StatusCode}");
		return res;
	}

	private static readonly JsonSerializerOptions jsonOpt = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public static T ReadJson<T>(this RestResponse res)
	{
		var content = res.Content;
		if (content == null) throw new InvalidOperationException("Query has no content");
		var obj = JsonSerializer.Deserialize<T>(content!, jsonOpt);
		return obj!;
	}
	
	public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, jsonOpt);
	
	public static string ReadString(this RestResponse res)
	{
		var content = res.Content;
		if (content == null) throw new InvalidOperationException("Query has no content");
		return content;
	}
	
	private static string MkUrl(string query) => BaseUrl + query;
}



public static object ToDump(object o) => o switch
{
	R e => $"{e}",
	ApiBatchRunResult e => new
	{
		PageCount = e.PageCount,
		VideosFound = e.VideosFound.Length
	},
	_ => o
};
