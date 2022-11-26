using LINQPad.Controls;
using LINQPad;
using PowMaybe;
using PowRxVar;

namespace LINQPadExtras.Utils;

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

	public static CheckBox CheckBox(string str, IRwVar<bool> v) => new(str, v.V, _ => v.V = !v.V);

	public static TextBox TextBox(string str, IRwVar<string> v) => new(str, v.V, c => v.V = c.Text);

	public static SelectBox SelectBox<TEnum>(IRwVar<TEnum> v) where TEnum : struct, Enum
	{
		var vals = Enum.GetValues<TEnum>();
		var idx = vals.IndexOfMaybe(e => e.Equals(v.V)).Ensure();
		return new SelectBox(
			vals.Cast<object>().ToArray(),
			idx,
			sb => v.V = (TEnum)sb.SelectedOption
		);
	}
}