<Query Kind="Program">
  <Reference>C:\Dev_Nuget\Libs\LINQPadExtras\Libs\LINQPadExtras\bin\Debug\net7.0-windows\LINQPadExtras.dll</Reference>
  <NuGetReference>CliWrap</NuGetReference>
  <Namespace>CliWrap</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LINQPadExtras</Namespace>
  <Namespace>LINQPadExtras.DialogLogic</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>LINQPadExtras.DialogLogic.Enums</Namespace>
</Query>

const string file = @"C:\tmp\dd\lock.txt";
const string folder = @"C:\tmp\dd";

void Main()
{
	Dialoger.Run(
		"Test",
		opt =>
		{
			
		},
		dlg =>
		{
			var cancelBtn = new Button("Cancel", _ => dlg.Close());
			dlg.AddButton(DlgBtnLocation.FooterLeft, DlgBtnType.Normal, cancelBtn);

			var scrollBtn = new Button("Scroll", _ =>
			{
				var scrollHeight = dlg.ScrollDiv.HtmlElement.GetAttribute("scrollHeight");
				var scrollTop = dlg.ScrollDiv.HtmlElement.GetAttribute("scrollTop");
				
				$"h:{scrollHeight} t:{scrollTop}".Dump();
				
				//var scrollHeight = int.Parse(scrollHeightStr);
				dlg.ScrollDiv.HtmlElement.SetAttribute("scrollTop", scrollHeight);

			});
			dlg.AddButton(DlgBtnLocation.FooterRight, DlgBtnType.Normal, scrollBtn);
		},
		dlg =>
		{
			for (var i = 0; i < 20; i++)
			{
				dlg.DC.AppendContent($"Item_{i}");
				Thread.Sleep(100);
			}
		}
	);
}
