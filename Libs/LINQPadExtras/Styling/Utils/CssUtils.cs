using LINQPad;
using System.Runtime.CompilerServices;
using LINQPadExtras.Utils;

namespace LINQPadExtras.Styling.Utils;

static class CssUtils
{
    private static readonly Dictionary<string, string> varMap = new();
    private static readonly HashSet<string> nonVarSet = new();

    private static void OnRestart()
    {
	    varMap.Clear();
	    nonVarSet.Clear();
	    //Util.HtmlHead.AddStyles(ExtrasConsts.BaseCss);
    }


    public static void SetVarJS(string name, string val) => Util.InvokeScript(false, "eval", $"document.documentElement.style.setProperty('--{name}', '{val}')");
    public static string GetVarJS(string name) => (string)Util.InvokeScript(true, "eval", $"document.documentElement.style.getPropertyValue('--{name}')");

    public static string GetVar(string val, [CallerArgumentExpression(nameof(val))] string? valExpr = null)
    {
        RestartDetector.OnRestart(nameof(CssUtils), OnRestart);

        var varName = GetValName(valExpr);
        if (varName == null || nonVarSet.Contains(varName)) return val;

        if (varMap.TryGetValue(varName, out var existingVal))
        {
            SetVarJS(varName, val);
            varMap[varName] = val;
        }
        else if (val != existingVal)
        {
	        varMap.Remove(varName);
	        nonVarSet.Add(varName);
	        return val;
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