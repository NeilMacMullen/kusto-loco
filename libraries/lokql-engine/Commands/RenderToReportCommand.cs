using CommandLine;


namespace Lokql.Engine.Commands;

public static class RenderToReportCommand
{
    internal static async Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;

        var result = exp._resultHistory.Fetch(o.ResultName);
        await exp.ActiveReport.UpdateOrAddImage(o.Name,exp,result);
    }

    [Verb("renderToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public string Name { get; set; } = string.Empty;
        [Value(1)] public string ResultName { get; set; } = string.Empty;
    }
}
