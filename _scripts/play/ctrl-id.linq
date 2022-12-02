<Query Kind="Program">
  <Namespace>LINQPad.Controls</Namespace>
</Query>

#load ".\..\libs\html-utils"


void Main()
{
	var ctrl = new CheckBox("Choose");
	ctrl.GetId()
		.IncId()
		.Dump();
	ctrl.Dump();

	var body = HtmlUtils.GetBody();
	Util.FixedFont(body).Dump();
}

