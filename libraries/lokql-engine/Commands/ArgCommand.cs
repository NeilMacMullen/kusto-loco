using AppInsightsSupport;
using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;


/// <summary>
///     Issues a query against ARG
/// </summary>
public static class ArgCommand
{

    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var blocks = context.InputProcessor;
        var settings = context.Settings;

        if (blocks.IsComplete)
            return;

        var query = blocks.ConsumeNextBlock();

        console.Info("Running ARG query.  This may take a while....");
        var al = new KqlClient(settings, console);
        var result= await al.LoadKqlAsync(KqlClient.KqlServiceType.Arg,o.ResourcePath,query);
        await context.InjectResult(result);
    }

    [Verb("arg", 
        HelpText = """
                   Runs a query against ARG (Azure Resource Graph).
                   The subscriptionId is a guid, optionally prefixed a tenandID and colon

                   Examples:
                   .set sub 12a.... 
                   .arg $sub 
                   Resources 
                   | summarize count() by type
                   """)]
    internal class Options
    {
        [Value(0, Required = true)]
        public string ResourcePath { get; set; } = string.Empty;
    }

}
