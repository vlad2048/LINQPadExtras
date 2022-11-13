using LINQPadExtras.Styling.Utils;

namespace LINQPadExtras.Utils;

static class RestartDetector
{
    private const string VarName = "restart-detector";

    public static void OnRestart(string name, Action action)
    {
        var val = CssUtils.GetVarJS(VarName);
        if (val.Contains(name)) return;
        action();
        CssUtils.SetVarJS(VarName, $"{val}{name}");
    }
}