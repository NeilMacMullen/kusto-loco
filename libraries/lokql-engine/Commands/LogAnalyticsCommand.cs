using AppInsightsSupport;
using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Issues a query against ARG
/// </summary>
public static class LogAnalyticsCommand
{

    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var blocks = context.InputProcessor;
        var settings = context.Settings;

        if (blocks.IsComplete)
            return;

        var query = blocks.ConsumeNextBlock();

        console.Info("Running Log Analytics query.  This may take a while....");
        var al = new KqlClient(settings, console);
        var result = await al.LoadKqlAsync(KqlClient.KqlServiceType.LogAnalytics, o.WorkspaceId, query);
        await context.InjectResult(result);
    }

    [Verb("loganalytics",
        HelpText = """
                   Runs a query against Log Analytics in Azure Monitor.
                   The WorkspaceId is a guid

                   Examples:
                   .set wkspc 12a.... 
                   .loganalytics $wkspc 
                   AppTraces 
                   | summarize count() by bin(timestamp, 1h)
                   """)]
    internal class Options
    {
        [Value(0, Required = true)]
        public string WorkspaceId { get; set; } = string.Empty;
    }

}
