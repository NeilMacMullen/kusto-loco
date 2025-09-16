using AppInsightsSupport;
using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Issues a query against ARG
/// </summary>
public static class DefenderCommand
{

    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var blocks = context.InputProcessor;
        var settings = context.Settings;

        if (blocks.IsComplete)
            return;

        var query = blocks.ConsumeNextBlock();

        console.Info("Running Defender query.  This may take a while....");
        var al = new KqlClient(settings, console);
        var result = await al.LoadKqlAsync(KqlClient.KqlServiceType.Defender, string.Empty, query);
        await context.InjectResult(result);
    }

    [Verb("defender",
        HelpText = """
                   Runs a query against Defender.

                   Examples:
                   .defender 
                   DeviceProcessEvents
                   | summarize count() by bin(Timestamp, 1h)
                   """)]
    internal class Options
    {
    }

}
