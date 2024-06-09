using AppInsightsSupport;
using CommandLine;

namespace Lokql.Engine.Commands;

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

        var result = await ai.LoadTable(o.Rid, query, t, DateTime.UtcNow);
        exp.InjectResult(result);
    }

    [Verb("appinsights", aliases: ["ai"],
        HelpText = "Runs a query against an application insights log-set")]
    internal class Options
    {
        [Value(0, HelpText = "resourceId", Required = true)]
        public string Rid { get; set; } = string.Empty;
        [Value(1, HelpText = "timespan ", Required = true)]
        public string Timespan { get; set; } = string.Empty;
    }
}
