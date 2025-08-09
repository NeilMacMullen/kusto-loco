using System.Collections.Immutable;
using CommandLine;
using KustoLoco.Core.Diagnostics;
using KustoLoco.FileFormats;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class GetEventLogCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
   
        var tableName = o.As;
        //remove table if it already exists
        if (exp.GetCurrentContext().HasTable(tableName)) exp.GetCurrentContext().RemoveTable(tableName);

        try
        {
            var events = EventLog.GetEvents().ToImmutableArray();
            if (o.Clear)
                EventLog.Clear();
            exp.GetCurrentContext().WrapDataIntoTable(tableName, events);
            exp.Info($"Loaded {NameEscaper.EscapeIfNecessary(tableName)}");
        }
        catch (Exception ex)
        {
            exp.Warn($"Data malformed: {ex.Message}");
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
