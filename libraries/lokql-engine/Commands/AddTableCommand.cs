using CommandLine;
using KustoLoco.FileFormats;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class AddTableCommand
{
    internal static Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        var queryContext = econtext.QueryContext;
        var blocks = econtext.InputProcessor;
        var settings = econtext.Settings;
        
        if (blocks.IsComplete)
        {
            console.Warn("No data provided.");
            return Task.CompletedTask;
        }

        var csvData = blocks.ConsumeNextBlock();

        var tableName = o.As;
        //remove table if it already exists
        if (queryContext.HasTable(tableName)) queryContext.RemoveTable(tableName);

        try
        {
            var serializer = CsvSerializer.Default(settings, console);
            var source = serializer.LoadFromString(csvData, tableName);
            queryContext.AddTable(source);
            console.Info($"Loaded {NameEscaper.EscapeIfNecessary(tableName)}");
        }
        catch (Exception ex)
        {
            console.Warn($"Data malformed: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    [Verb("addtable", aliases: ["csvdata"],
        HelpText = @"loads data from formatted inline text and adds a table
The first line is assumed to be the header row.  Separators are inferred from
the first row and items are trimmed by default
Examples:
  .addtable ages
  Name   | Age
  Alice  | 30
  Bob    | 28

The table name is optional and defaults to 'data' if not provided.
"
    )]
    internal class Options
    {
        [Value(0, HelpText = "Name of table (defaults to 'data')")]
        public string As { get; set; } = "data";
    }
}
