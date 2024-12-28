using CommandLine;

namespace Lokql.Engine.Commands;

public static class RenderTableToText
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var res = exp._resultHistory.Fetch(o.ResultName);
        var text = (res.RowCount > 0) ? res.Get(0, 0)?.ToString()??"<null>" : "no data";
        exp.ActiveReport.UpdateOrAddText(o.Name, text);
        return Task.CompletedTask;
    }

    [Verb("renderTextToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public string Name { get; set; } = string.Empty;
        [Value(1)] public string ResultName { get; set; } = string.Empty;
    }
}
