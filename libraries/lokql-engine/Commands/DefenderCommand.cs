using AppInsightsSupport;
using CommandLine;
using KustoLoco.PluginSupport;
using NotNullStrings;

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
        var tenant = o.TenantId;
        if (tenant.IsNotBlank())
            tenant = tenant + ":";
        var result = await al.LoadKqlAsync(KqlClient.KqlServiceType.Defender, string.Empty, query);
        await context.InjectResult(result);
    }

    [Verb("defender",
        HelpText = """
                   Runs a query against Defender.
                   A tenantID can optionally be provided as the first argument

                   Examples:
                   .defender 
                   DeviceProcessEvents
                   | summarize count() by bin(Timestamp, 1h)
                   """)]
    internal class Options
    {
        [Value(0, Required = false)]
        public string TenantId { get; set; } = string.Empty;
    }

}
