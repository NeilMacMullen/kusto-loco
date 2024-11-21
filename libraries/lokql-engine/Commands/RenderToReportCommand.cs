using CommandLine;
using KustoLoco.Rendering;

namespace Lokql.Engine.Commands;

public static class RenderToReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;

        exp.ActiveReport.UpdateOrAddImage(o.Name,exp);
       
        return Task.CompletedTask;
    }

    [Verb("renderToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Option] public string Name { get; set; } = string.Empty;
    }
}
