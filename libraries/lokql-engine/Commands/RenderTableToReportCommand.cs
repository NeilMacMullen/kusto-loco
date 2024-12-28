using CommandLine;

namespace Lokql.Engine.Commands;

public static class RenderTableToReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
       
        exp.ActiveReport.UpdateOrAddTable(o.Name, exp._resultHistory.Fetch(o.ResultName));
        

        return Task.CompletedTask;
    }

    [Verb("renderTableToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public string Name { get; set; } = string.Empty;
        [Value(1)] public string ResultName { get; set; } = string.Empty;
    }
}
