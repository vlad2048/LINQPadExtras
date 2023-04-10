using System.Text.Json;
using System.Text.Json.Serialization;

namespace LINQPadExtras.PageServing.Utils;

static class JsonUtils
{
	private static readonly JsonSerializerOptions jsonOpt = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};
	static JsonUtils() => jsonOpt.Converters.Add(new JsonStringEnumConverter());
	
	public static T Deser<T>(this string str) => JsonSerializer.Deserialize<T>(str, jsonOpt)!;
	public static string Ser<T>(this T obj) => JsonSerializer.Serialize(obj, jsonOpt);
}