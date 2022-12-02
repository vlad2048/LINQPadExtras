<Query Kind="Program">
  <NuGetReference>LINQPadExtras</NuGetReference>
  <Namespace>PowRxVar</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
</Query>

void Main()
{
	var bndVar = Var.MakeBnd(false);
	
	var ctrl = XUtil.CheckBox("Choose", bndVar);
	var dc = new DumpContainer();
	var btn = new Button("Switch", _ => bndVar.SetOuter(!bndVar.V));
	bndVar.Subscribe(_val => dc.UpdateContent($"{_val}"));	

	Util.VerticalRun(ctrl, dc, btn).Dump();
}

static class XUtil
{
	public static CheckBox CheckBox(string text, IFullRwBndVar<bool> bndVar)
	{
		var ctrl = new CheckBox("", bndVar.V, c => bndVar.SetInner(c.Checked));
		bndVar.WhenOuter.Subscribe(_val => ctrl.Checked = _val);
		return ctrl;
	}
}