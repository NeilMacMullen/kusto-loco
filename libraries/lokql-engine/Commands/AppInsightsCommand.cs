using AppInsightsSupport;
using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
///     Issues a query against an application insights server
/// </summary>
public static class AppInsightsCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var blocks = econtext.Sequence;
        var ai = new ApplicationInsightsLogLoader(exp.Settings, exp._outputConsole);

        var timespan = o.Timespan;
        if (!TimeRangeProcessor.ParseTime(timespan, out var t))
        {
            exp.Warn($"Unable to parse timespan '{timespan}'");
            return;
        }

        if (blocks.Complete)
            return;

        var query = blocks.Next();
        //make sure we pick up any variable interpolation in case we are inside a function
        query = exp._interpolator.Interpolate(query);
        exp.Info("Running application insights query.  This may take a while....");
        var result = await ai.LoadTable(o.Rid, query, t, DateTime.UtcNow);
        await exp.InjectResult(result);
    }

    [Verb("appinsights", aliases: ["ai"],
        HelpText = @"Runs a query against application insights
The resourceId is the full resourceId of the application insights instance which can be obtained
from the JSON View of the Insights resource in the Azure portal.
The timespan is a duration string which can be in the format of '7d', '30d', '1h' etc.
If not specified, the default is 24 hours.

Examples:
 .set appservice /subscriptions/12a.... 
 .appinsights $appservice 7d
 traces | where message contains 'error'

 .appinsights $appservice 30d
 exceptions
 | summarize count() by outerMessage
 | render piechart
")]
    internal class Options
    {
        [Value(0, HelpText = "resourceId", Required = true)]
        public string Rid { get; set; } = string.Empty;

        [Value(1, HelpText = "timespan ")] public string Timespan { get; set; } = "1d";
    }
}
