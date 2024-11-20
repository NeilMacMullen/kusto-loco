using CommandLine;
using KustoLoco.Rendering;

namespace Lokql.Engine.Commands;

public static class RenderToReportCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;

        var result = exp._prevResult;
        var renderer = new KustoResultRenderer(exp.Settings);
        renderer.RenderToComposer(result, exp.ActiveReport.Composer);
        return Task.CompletedTask;
    }

    [Verb("renderToReport", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
    }
}
