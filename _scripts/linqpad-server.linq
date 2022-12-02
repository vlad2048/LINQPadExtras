<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0-windows\LINQPadExtras.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>WebSocketSharp-netstandard</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Disposables</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>WebSocketSharp</Namespace>
  <Namespace>WebSocketSharp.Server</Namespace>
  <Namespace>LINQPadExtras.PageServing</Namespace>
  <Namespace>PowMaybe</Namespace>
</Query>

static Disp d = null!;

void Main()
{
	//LINQPadServer.Start(opt => { opt.HtmlEditFolder = @"C:\Dev_Nuget\Libs\LINQPadExtras\_infos\cmp\ver0"; });
	Css.Init();
	
	d = new Disp();
	
	Util.VerticalRun(
		Util.HorizontalRun(true,
			MkCheckBox(),
			MkTextBox()
		),
		Util.HorizontalRun(true,
			MkButton(),
			MkLinkButton(),
			MkImageButton()
		),
		Util.HorizontalRun(true,
			MkImage(),
			MkStyles()
		)
	).Dump();
}

void Main1() { Util.ReadLine(); Main(); }

public static void SetJS(string name, string val) => Util.InvokeScript(false, "eval", $"document.documentElement.style.setProperty('--{name}', '{val}')");


Control MkCheckBox()
{
	var isOn = Var.MakeBnd(false).D(d);
	var ctrl = Ctrl.CheckBox("IsOn", isOn);
	var dc = new DumpContainer();
	isOn.Subscribe(_isOn => dc.Content = $"{_isOn}").D(d);
	return new FieldSet("CheckBox", new WrapPanel(ctrl, dc));
}

Control MkTextBox()
{
	var txt = Var.MakeBnd("initial").D(d);
	var ctrl = Ctrl.TextBox(txt);
	var dc = new DumpContainer();
	txt.Subscribe(_txt => dc.Content = _txt).D(d);
	return new FieldSet("TextBox", new WrapPanel(ctrl, dc));
}

Control MkButton()
{
	var selItem = Var.Make(May.None<string>()).D(d);
	var ctrl = Enumerable.Range(0, 2).Select(e => $"item_{e}").Select(e => Ctrl.Button(e, () => selItem.V = May.Some(e)));
	var dc = new DumpContainer();
	selItem.Subscribe(v => dc.UpdateContent($"{v}")).D(d);
	return new FieldSet("LinkButton", new StackPanel(false, new DumpContainer(ctrl), dc));
}

Control MkLinkButton()
{
	var selItem = Var.Make(May.None<string>()).D(d);
	var ctrl = Enumerable.Range(0, 2).Select(e => $"item_{e}").Select(e => Ctrl.LinkButton(e, () => selItem.V = May.Some(e)));
	var dc = new DumpContainer();
	selItem.Subscribe(v => dc.UpdateContent($"{v}")).D(d);
	return new FieldSet("LinkButton", new StackPanel(false, new DumpContainer(ctrl), dc));
}

Control MkImageButton()
{
	var selItem = Var.Make("_").D(d);
	var dc = new DumpContainer();
	selItem.Subscribe(item => dc.Content = item).D(d);
	return new FieldSet("ImageButton", new StackPanel(false,
		Ctrl.ImageButton(Consts.ImgVS, () => selItem.V = "VS"),
		Ctrl.ImageButton(Consts.ImgVSCode, () => selItem.V = "VSCode"),
		dc
	));
}

Control MkImage() => new FieldSet("Image", new WrapPanel(
	new Image(Consts.ImgVS),
	new Image(Consts.ImgVSCode)
));

Control MkStyles()
{
	var btnStyle = Ctrl.Button("Add Style", () => Util.HtmlHead.AddStyles("""
		.fmt {
			display: flex;
			flex-direction: column;
		}
	"""));
	var divStyles = new Div(
		new Span("First"),
		new Span("Second")
	)
	{
		CssClass = "fmt"
	};
	var btnVar = Ctrl.Button("Add Css var", () => SetJS("cool", "red"));
	var btnVarChange = Ctrl.Button("Change Css var", () => SetJS("cool", "blue"));
	var divVar = new Div(new Span("color"));
	divVar.Styles["color"] = "var(--cool)";
	return new FieldSet("Css",
		new StackPanel(true,
			new FieldSet("Style", new StackPanel(false, btnStyle, divStyles)),
			new FieldSet("Vars", new StackPanel(false, btnVar, btnVarChange, divVar))
		)
	);
}


static class PathUtils
{
	public static string GetImage(string name) =>
		Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath)!, "art", name);
}

static class Consts
{
	public static string ImgVS => PathUtils.GetImage("icon-vs.png");
	public static string ImgVSCode => PathUtils.GetImage("icon-vscode.png");
}

static class Css
{
	public static void Init() => Util.HtmlHead.AddStyles("""
		thead>tr:first-of-type {
			display: none;
		}
		/*td {
			vertical-align: middle;
		}*/
		legend {
			background: transparent;
		}
		div:has(> fieldset) {
			display: flex;
		}
		
		fieldset {
			border: 1pt solid #797979ba;
			border-radius: 5px;
			background-color: #323232;
			box-shadow: 0px 0px 4px 2px #2662ad57;
		}
		"""
	);
}