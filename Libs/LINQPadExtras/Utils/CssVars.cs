using System.Runtime.CompilerServices;
using LINQPad;

namespace LINQPadExtras.Utils;

static class CssVars
{
	private static readonly Dictionary<string, string> varMap = new();
	private static readonly HashSet<string> nonVarSet = new();

	public static void Init()
	{
		varMap.Clear();
		nonVarSet.Clear();
	}

	public static void SetJS(string name, string val) => Util.InvokeScript(false, "eval", $"document.documentElement.style.setProperty('--{name}', '{val}')");
	public static string GetJS(string name) => (string)Util.InvokeScript(true, "eval", $"document.documentElement.style.getPropertyValue('--{name}')");

	public static string Get(string val, [CallerArgumentExpression(nameof(val))] string? valExpr = null)
	{
		var varName = GetValName(valExpr);
		if (varName == null || nonVarSet.Contains(varName)) return val;

		if (varMap.TryGetValue(varName, out var existingVal))
		{
			if (val != existingVal)
			{
				varMap.Remove(varName);
				nonVarSet.Add(varName);
				return val;
			}
		}
		else
		{
			SetJS(varName, val);
			varMap[varName] = val;
		}
		return $"var(--{varName})";
	}

	private static string? GetValName(string? expr)
	{
		if (string.IsNullOrEmpty(expr)) return null;
		var c = expr[0];
		if (c is '$' or '@' or '"') return null;
		if (expr.Contains(' ')) return null;
		return expr
			.Replace(".", "");
	}
}