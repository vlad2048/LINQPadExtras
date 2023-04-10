namespace LINQPadExtras.PageServing;

static class PageServingConsts
{
	public static readonly TimeSpan RefreshDelay = TimeSpan.FromMilliseconds(100);
	public static readonly TimeSpan MissedRefreshInterval = TimeSpan.FromMilliseconds(1235);
	public static readonly TimeSpan MissedRefreshWait = TimeSpan.FromMilliseconds(617);

	public const string DataAttrName = "data-sync";
}