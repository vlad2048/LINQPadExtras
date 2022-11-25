using System.Runtime.CompilerServices;
using LINQPad;
using LINQPad.Controls;
using PowRxVar;

namespace LINQPadExtras.Utils.Exts;

public static class ControlSettersExt
{
	// ********
	// * Base *
	// ********
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


	// ***************
	// * Conditional *
	// ***************
	public static C SetIf<C>(this C ctrl, bool cond, string key, string val, string? valElse = null)
		where C : Control => cond switch {
			true => ctrl.Set(key, val),
			false => ctrl.SetOpt(key, valElse),
		};

	public static DumpContainer SetIf(this DumpContainer ctrl, bool cond, string key, string val, string? valElse = null)
		=> cond switch {
			true => ctrl.Set(key, val),
			false => ctrl.SetOpt(key, valElse),
		};

	public static C SetWhen<C>(this C ctrl, IRoVar<bool> cond, string key, string val, string? valElse = null)
		where C : Control
	{
		cond.Subscribe(e => {
			if (e)
				ctrl.Set(key, val);
			else
				ctrl.SetOpt(key, valElse);
		}).D(cond);
		return ctrl;
	}

	public static DumpContainer SetWhen(this DumpContainer ctrl, IRoVar<bool> cond, string key, string val, string? valElse = null)
	{
		cond.Subscribe(e => {
			if (e)
				ctrl.Set(key, val);
			else
				ctrl.SetOpt(key, valElse);
		}).D(cond);
		return ctrl;
	}



	public static C SetForeColorIf<C>(this C ctrl, bool cond, string col, string? colElse = null)
		where C : Control => cond switch
	{
		true => ctrl.SetForeColor(col),
		false => ctrl.SetForeColorOpt(colElse),
	};

	public static DumpContainer SetForeColorIf(this DumpContainer ctrl, bool cond, string col, string? colElse = null)
		=> cond switch 
		{
			true => ctrl.SetForeColor(col),
			false => ctrl.SetForeColorOpt(colElse),
		};

	public static C SetForeColorWhen<C>(this C ctrl, IRoVar<bool> cond, string col, string? colElse = null)
		where C : Control
	{
		cond.Subscribe(e => {
			if (e)
				ctrl.SetForeColor(col);
			else
				ctrl.SetForeColorOpt(colElse);
		}).D(cond);
		return ctrl;
	}

	public static DumpContainer SetForeColorWhen(this DumpContainer ctrl, IRoVar<bool> cond, string col, string? colElse = null)
	{
		cond.Subscribe(e => {
			if (e)
				ctrl.SetForeColor(col);
			else
				ctrl.SetForeColorOpt(colElse);
		}).D(cond);
		return ctrl;
	}




	



	public static C SetBackColorIf<C>(this C ctrl, bool cond, string col, string? colElse = null)
		where C : Control => cond switch
	{
		true => ctrl.SetBackColor(col),
		false => ctrl.SetBackColorOpt(colElse),
	};

	public static DumpContainer SetBackColorIf(this DumpContainer ctrl, bool cond, string col, string? colElse = null)
		=> cond switch 
		{
			true => ctrl.SetBackColor(col),
			false => ctrl.SetBackColorOpt(colElse),
		};

	public static C SetBackColorWhen<C>(this C ctrl, IRoVar<bool> cond, string col, string? colElse = null)
		where C : Control
	{
		cond.Subscribe(e => {
			if (e)
				ctrl.SetBackColor(col);
			else
				ctrl.SetBackColorOpt(colElse);
		}).D(cond);
		return ctrl;
	}

	public static DumpContainer SetBackColorWhen(this DumpContainer ctrl, IRoVar<bool> cond, string col, string? colElse = null)
	{
		cond.Subscribe(e => {
			if (e)
				ctrl.SetBackColor(col);
			else
				ctrl.SetBackColorOpt(colElse);
		}).D(cond);
		return ctrl;
	}



	// **********
	// * Colors *
	// **********
	public static C SetForeColor<C>(this C ctrl, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) where C : Control =>
		ctrl.Set("color", CssVars.Get(col, valExpr));

	public static DumpContainer SetForeColor(this DumpContainer dc, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) =>
		dc.Set("color", CssVars.Get(col, valExpr));
	
	public static C SetForeColorOpt<C>(this C ctrl, string? col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) where C : Control => col switch
	{
		null => ctrl,
		not null => ctrl.SetForeColor(col)
	};

	public static DumpContainer SetForeColorOpt(this DumpContainer ctrl, string? col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) => col switch
	{
		null => ctrl,
		not null => ctrl.SetForeColor(col)
	};




	public static C SetBackColor<C>(this C ctrl, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) where C : Control =>
		ctrl.Set("background-color", CssVars.Get(col, valExpr));

	public static DumpContainer SetBackColor(this DumpContainer dc, string col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) =>
		dc.Set("background-color", CssVars.Get(col, valExpr));

	public static C SetBackColorOpt<C>(this C ctrl, string? col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) where C : Control => col switch
	{
		null => ctrl,
		not null => ctrl.SetBackColor(col)
	};

	public static DumpContainer SetBackColorOpt(this DumpContainer ctrl, string? col, [CallerArgumentExpression(nameof(col))] string? valExpr = null) => col switch
	{
		null => ctrl,
		not null => ctrl.SetBackColor(col)
	};




	public static C SetColors<C>(this C ctrl, string colFore, string colBack, [CallerArgumentExpression(nameof(colFore))] string? valExprFore = null, [CallerArgumentExpression(nameof(colBack))] string? valExprBack = null) where C : Control =>
		ctrl
			.SetForeColor(colFore, valExprFore)
			.SetBackColor(colBack, valExprBack);

	public static DumpContainer SetColors(this DumpContainer dc, string colFore, string colBack, [CallerArgumentExpression(nameof(colFore))] string? valExprFore = null, [CallerArgumentExpression(nameof(colBack))] string? valExprBack = null) =>
		dc
			.SetForeColor(colFore, valExprFore)
			.SetBackColor(colBack, valExprBack);


	// **********
	// * Events *
	// **********
	public static C OnClick<C>(this C ctrl, Action action) where C : Control
	{
		ctrl.Click += (_, _) => action();
		return ctrl;
	}


	// ***********
	// * Private *
	// ***********
	private static C SetOpt<C>(this C ctrl, string key, string? val)
		where C : Control => val switch {
			not null => ctrl.Set(key, val),
			null => ctrl
		};

	private static DumpContainer SetOpt(this DumpContainer ctrl, string key, string? val)
		=> val switch {
			not null => ctrl.Set(key, val),
			null => ctrl
		};

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