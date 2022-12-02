namespace LINQPadExtras.PageServing.Structs;

enum ChgType
{
	Click,
	CheckBox,
	TextBox,
}

record Chg(ChgType Type, string Id, string Val);
