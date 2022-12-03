<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0-windows\LINQPadExtras.dll</Reference>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>PowBasics.ColorCode</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>LINQPadExtras.Utils.Exts</Namespace>
</Query>

public const bool EnableBg = true;

void Main()
{
	Css.Init();
	
	/*new Div().StyleRootVert().Bg().WithChildren(
	
		new Div().StyleHoriz().Bg().WithChildren(
			new Span("Icon"),
			new Span("Title").StyleStretchHorizAndCenterText().Bg(),
			new Span("Link")
		),
		
		new Div().StyleFillHoriz().Bg().WithChildren(
			new Div().StyleWidthFillVert(200).Bg(),
			new Div().StyleRootVert().Bg()
		)
	
	)
	
		.Dump();*/
	
	var c = new CtrlMaker(opt =>
	{
		
	});
	
	
	c.Div().LayOutFill()
}


internal enum LayInType
{
	Horiz,
	Vert,
	VertScroll,
}

static class Styler
{
	private static readonly string[] colors = ColorUtils.MakePalette(11, 7).SelectToArray(ToHexCol);
	private static int colorIdx;
	private static string GetCol() { var col = colors[colorIdx++]; colorIdx = colorIdx % colors.Length; return col; }
	private static string ToHexCol(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
	private static void AddStyle(string css) => Util.HtmlHead.AddStyles(css);
	
	public static C Bg<C>(this C ctrl, string? col = null) where C : Control => EnableBg switch
	{
		true => ctrl.Set("background-color", col ?? GetCol()),
		false => ctrl
	};
	
	
	public static C LayOutFill<C>(this C ctrl, string? flex = null) where C : Control => ctrl.Set("width", "100%").Set("height", "100%");
	
	public static C LayOut<C>(this C ctrl, string? flex = null) where C : Control => ctrl.SetOpt("flex", flex);
	
	public static C LayIn<C>(this C ctrl, LayInType type) where C : Control => type switch
	{
		LayInType.Horiz => ctrl.Set("display", "flex"),
		LayInType.Vert => ctrl.Set("display", "flex").Set("flex-direction", "column"),
		LayInType.VertScroll => ctrl.Set("display", "flex").Set("flex-direction", "column").Set("o,
		_ => throw new ArgumentException(),
	};
	
	
	
	private static C SetOpt<C>(this C ctrl, string styleName, string? styleVal) where C : Control => styleVal switch
	{
		not null => ctrl.Set(styleName, styleVal),
		null => ctrl
	};
	
	private static C Set<C>(this C ctrl, string styleName, string styleVal) where C : Control
	{
		ctrl.Styles[styleName] = styleVal;
		return ctrl;
	}
	
	
	
	
	/*public static C Sz<C>(this C ctrl, Dim dimX, Dim dimY) where C : Control
	{
		string? Dim2Css(Dim d) => d.Type switch
		{
			Dim.Fit => null,
		}
	}
	
	public static Div WithChildren(this Div ctrl, params Control[] children)
	{
		ctrl.Children.AddRange(children);
		return ctrl;
	}
	
	public static C StyleRootVert<C>(this C ctrl) where C : Control =>
		ctrl.SetClass("style-root-vert", """
			width: 100%;
			height: 100%;
			display: flex;
			flex-direction: column;
		""");
	
	public static C StyleHoriz<C>(this C ctrl) where C : Control =>
		ctrl.SetClass("style-horiz", """
			width: 100%;
			display: flex;
			align-items: baseline;
		""");
	
	public static C StyleFillHoriz<C>(this C ctrl) where C : Control =>
		ctrl.SetClass("style-fill-horiz", """
			width: 100%;
			height: 100%;
			display: flex;
			align-items: baseline;
		""");
	
	public static C StyleFillVert<C>(this C ctrl) where C : Control =>
		ctrl.SetClass("style-fill-vert", """
			width: 100%;
			height: 100%;
			display: flex;
			flex-direction: column;
			align-items: baseline;
		""");

	public static C StyleWidthFillVert<C>(this C ctrl, int width) where C : Control =>
		ctrl.SetClass("style-width-fill-horiz", $"""
			width: {width}px;
			height: 100%;
			display: flex;
			flex-direction: column;
			align-items: baseline;
		""");
		
		
		
	
	public static C StyleStretchHorizAndCenterText<C>(this C ctrl) where C : Control =>
		ctrl.SetClass("style-stretch-horiz-and-center-text", """
			width: 100%;
			text-align: center;
		""");
	
	public static C SetAutoLeft<C>(this C ctrl) where C : Control => ctrl.Set("margin-left", "auto");
	
	private static C SetClass<C>(this C ctrl, string clsName, string clsCss) where C : Control {
		ctrl.CssClass = clsName;
		Util.HtmlHead.AddStyles($$"""
			.{{clsName}} {
				{{clsCss}}
			}
			""");
		return ctrl;
	}
	private static void AddStyle(string css) => Util.HtmlHead.AddStyles(css);
	private static string ToHexCol(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";*/
}




class CtrlMakerOpt
{
	public bool UseBg { get; set; } = true;
	public int Gap { get; set; } = 5;
	internal static CtrlMakerOpt Build(Action<CtrlMakerOpt>? optFun = null) { var opt = new CtrlMakerOpt(); optFun?.Invoke(opt); return opt; }
}

class CtrlMaker
{
	private readonly CtrlMakerOpt opt;
	
	public CtrlMaker(Action<CtrlMakerOpt>? optFun = null)
	{
		opt = CtrlMakerOpt.Build(optFun);
	}

	public Div Div()
	{
		var div = new Div();
		return opt.UseBg switch
		{
			false => div,
			true => div.Bg()
		};
	}
}



static class Css
{
	public static void Init() => Util.HtmlHead.AddStyles("""
		*, *::before, *::after {
			box-sizing: border-box;
		}
		* {
			margin: 0 !important;
			padding: 0;
			border: 0;
		}
		html, body {
			height: 100%;
		}
		body > div {
			height: 100%;
		}
		body {
			padding: 2px;
		}
		""");
}






internal enum Dir
{
	Horiz,
	Vert,
}

internal enum DimType
{
	Fit,
	Fix,
	Fill,
	Scroll
};
internal readonly record struct Dim
{
	private readonly int val;
	public DimType Type { get; }
	public int Val => Type switch
	{
		DimType.Fix => val,
		_ => throw new ArgumentException()
	};
	private Dim(DimType type, int val)
	{
		Type = type;
		this.val = val;
	}
	
	public static readonly Dim Fit = new(DimType.Fit, 0);
	public static Dim Fix(int val) => new(DimType.Fix, val);
	public static readonly Dim Fill = new(DimType.Fill, 0);	
	public static readonly Dim Scroll = new(DimType.Scroll, 0);	
}




