namespace LINQPadExtras.Scripting_Batcher.Panels.Ctrls;

static class StatusBtnConsts
{
	public static readonly TimeSpan StatusSpinnerInterval = TimeSpan.FromMilliseconds(150);
	public const string StatusCol = "#cbd769";
	public static readonly char[] StatusSpinnerChars =
	{
		'⠟',
		'⠯',
		'⠷',
		'⠾',
		'⠽',
		'⠻'
	};
	public const char StatusNoneChar = ' ';
	public const char StatusOpenChar = '-';
	public const char StatusClosedChar = '+';
}