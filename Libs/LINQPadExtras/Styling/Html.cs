using LINQPad.Controls;
using LINQPadExtras.Styling.Utils;
using System.Runtime.CompilerServices;
using LINQPad;
using PowRxVar;

namespace LINQPadExtras.Styling;

public static class Html
{
	public static Div Div(string str) =>
		new()
		{
			HtmlElement =
			{
				InnerText = str
			}
		};

	public static Hyperlink Hyperlink(string str, Action action, IRoVar<bool> enabled) => new(str, _ =>
	{
		if (!enabled.V) return;
		action();
	});

	public static FieldSet FieldSet(string str, object obj) => new(str, new DumpContainer(obj));

	public static FieldSet FieldSet(string str, params Control[] ctrls) => new(str, new DumpContainer(ctrls));

	public static CheckBox CheckBox(string str, IRwVar<bool> v) => new(str, v.V, _ => v.V = !v.V);

	public static C Set<C>(this C ctrl, string attrName, string attrVal)
		where C : Control
	{
		ctrl.Styles[attrName] = attrVal;
		return ctrl;
	}
	public static DumpContainer Set(this DumpContainer dc, string attrName, string attrVal)
	{
		dc.Style = SetPropInString(dc.Style, attrName, attrVal);
		return dc;
	}

	public static C SetForeColor<C>(this C ctrl, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) where C : Control =>
		ctrl.Set("color", CssUtils.GetVar(col, valExpr));
	public static DumpContainer SetForeColor(this DumpContainer dc, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) =>
		dc.Set("color", CssUtils.GetVar(col, valExpr));

	public static C SetBackColor<C>(this C ctrl, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) where C : Control =>
		ctrl.Set("color", CssUtils.GetVar(col, valExpr));
	public static DumpContainer SetBackColor(this DumpContainer dc, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) =>
		dc.Set("background-color", CssUtils.GetVar(col, valExpr));

	public static C SetColors<C>(this C ctrl, string colFore, string colBack, [CallerArgumentExpression(nameof(colFore))] string? valExprFore = null, [CallerArgumentExpression(nameof(colBack))] string? valExprBack = null) where C : Control =>
		ctrl
			.SetForeColor(colFore, valExprFore)
			.SetBackColor(colBack, valExprBack);
	public static DumpContainer SetColors(this DumpContainer dc, string colFore, string colBack, [CallerArgumentExpression(nameof(colFore))] string? valExprFore = null, [CallerArgumentExpression(nameof(colBack))] string? valExprBack = null) =>
		dc
			.SetForeColor(colFore, valExprFore)
			.SetBackColor(colBack, valExprBack);


	private static string SetPropInString(string? s, string key, string val)
	{
		s ??= string.Empty;
		var look = $"{key}:";
		var i0 = s.IndexOf(look, StringComparison.Ordinal);
		if (i0 == -1) return $"{s}{look}{val}; ";
		var i1 = s.IndexOf(';', i0);
		if (i1 == -1) return s;
		return $"{s[..i0]}{look}{val}{s[i1..]}";
	}
}
