using LINQPadExtras.DialogLogic.Enums;

namespace LINQPadExtras.DialogLogic;

public class DialogerOpt
{
	public DlgType Type { get; set; } = DlgType.Normal;
	public bool Maximize { get; set; } = false;

	internal static DialogerOpt Make(Action<DialogerOpt>? optFun)
	{
		var opt = new DialogerOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}