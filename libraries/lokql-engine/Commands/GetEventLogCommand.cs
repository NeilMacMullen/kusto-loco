using System.Collections.Immutable;
using CommandLine;
using KustoLoco.Core.Diagnostics;
using KustoLoco.FileFormats;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class GetEventLogCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        var tableName = o.As;
        //remove table if it already exists
        if (queryContext.HasTable(tableName)) queryContext.RemoveTable(tableName);

        try
        {
            var events = EventLog.GetEvents().ToImmutableArray();
            if (o.Clear)
                EventLog.Clear();
            queryContext.WrapDataIntoTable(tableName, events);
            console.Info($"Loaded {NameEscaper.EscapeIfNecessary(tableName)}");
        }
        catch (Exception ex)
        {
            console.Warn($"Data malformed: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    [Verb("events",
        HelpText = @"Gets the event log"

    )]
    internal class Options
    {
        [Value(0, HelpText = "Name of table (defaults to 'data')")]
        public string As { get; set; } = "_events";
        [Option]
        public bool Clear { get; set; }
    }
}
