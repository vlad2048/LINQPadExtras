using System.Reactive.Linq;
using LINQPad.Controls;
using LINQPadExtras.PageServing.Structs;
using LINQPadExtras.PageServing.Utils.HtmlUtils;
using LINQPadExtras.Utils.Exts;
using PowRxVar;
using Image = LINQPad.Controls.Image;

namespace LINQPadExtras.PageServing;

public static class Ctrl
{
	public static CheckBox CheckBox(string text, IFullRwBndVar<bool> bndVar)
	{
		var ctrl = new CheckBox(text, bndVar.V);
		var id = ctrl.InputControl.HtmlElement.ID;

		// ClickInLINQPad -> SetInner
		ctrl.WhenClick().Subscribe(_ => bndVar.SetInner(ctrl.Checked)).D(LINQPadServer.MasterD);

		// ClickOnPage -> WhenChg -> SetOuter
		LINQPadServer.WhenChg.Where(e => e.Type == ChgType.CheckBox && e.Id == id).Subscribe(chg => bndVar.SetOuter(bool.Parse(chg.Val))).D(LINQPadServer.MasterD);

		// WhenOuter -> UpdateCtrl
		bndVar.WhenOuter.Subscribe(_val => ctrl.Checked = _val).D(LINQPadServer.MasterD);

		// WhenOuter or Inner -> Refresh
		bndVar.Subscribe(_ => LINQPadServer.SignalRefreshNeeded()).D(LINQPadServer.MasterD);


		ctrl.InputControl.HtmlElement.SetAttribute("onchange", MakeSend(ChgType.CheckBox, id, "this.checked"));
		LINQPadServer.Tweaks.SetTweak(id, n => n.SetChecked(bndVar.V));

		return ctrl;
	}

	public static TextBox TextBox(IFullRwBndVar<string> bndVar)
	{
		var ctrl = new TextBox(bndVar.V);
		var id = ctrl.HtmlElement.ID;

		// ClickInLINQPad -> SetInner
		ctrl.WhenTextInput().Subscribe(_ => bndVar.SetInner(ctrl.Text)).D(LINQPadServer.MasterD);

		// ClickOnPage -> WhenChg -> SetOuter
		LINQPadServer.WhenChg.Where(e => e.Type == ChgType.TextBox && e.Id == id).Subscribe(chg => bndVar.SetOuter(chg.Val)).D(LINQPadServer.MasterD);

		// WhenOuter -> UpdateCtrl
		bndVar.WhenOuter.Subscribe(_val => ctrl.Text = _val).D(LINQPadServer.MasterD);

		// WhenOuter or Inner -> Refresh
		bndVar.Subscribe(_ => LINQPadServer.SignalRefreshNeeded()).D(LINQPadServer.MasterD);


		//ctrl.HtmlElement.SetAttribute("oninput", MakeSend(ChgType.TextBox, id, "this.value"));
		ctrl.HtmlElement.SetAttribute("onchange", MakeSend(ChgType.TextBox, id, "this.value"));
		LINQPadServer.Tweaks.SetTweak(id, n => n.SetAttr("value", bndVar.V));

		return ctrl;
	}
	
	public static Button Button(string text, Action action) =>
		new Button(text).HookClick(action);

	public static Hyperlink LinkButton(string text, Action action)
	{
		var ctrl = new Hyperlink(text).HookClick(action);
		ctrl.HtmlElement.SetAttribute("href", null);
		return ctrl;
	}

	public static Hyperlink LinkButton(string text, Action action, IRoVar<bool> isEnabled)
	{
		void ActionWithEnable()
		{
			if (!isEnabled.V) return;
			action();
		}
		var ctrl = new Hyperlink(text).HookClick(ActionWithEnable);
		ctrl.HtmlElement.SetAttribute("href", null);
		return ctrl;
	}
	
	public static Image ImageButton(string imageFile, Action action) =>
		new Image(imageFile).HookClick(action);
	

	private static C HookClick<C>(this C ctrl, Action action) where C : Control
	{
		var id = ctrl.HtmlElement.ID;

		ctrl.Styles["cursor"] = "pointer";

		void ActionRefresh()
		{
			action();
			LINQPadServer.SignalRefreshNeeded();
		}

		ctrl.WhenClick().Subscribe(_ => ActionRefresh()).D(LINQPadServer.MasterD);
		LINQPadServer.WhenChg.Where(e => e.Type == ChgType.Click && e.Id == id).Subscribe(_ => ActionRefresh()).D(LINQPadServer.MasterD);

		ctrl.HtmlElement.SetAttribute("onclick", MakeSend(ChgType.Click, id));

		return ctrl;
	}

	private static string MakeSend(ChgType type, string id, string valExpr) =>
		$$"""send('{{type}}', '{{id}}', `${{{valExpr}}}`)""";

	private static string MakeSend(ChgType type, string id) =>
		$$"""send('{{type}}', '{{id}}', null)""";
}