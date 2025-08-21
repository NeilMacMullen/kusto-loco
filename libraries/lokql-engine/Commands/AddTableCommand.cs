using System.Collections.Immutable;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.FileFormats;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class AddTableCommand
{
    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        var blocks = context.InputProcessor;
        var settings = context.Settings;

        if (blocks.IsComplete) console.Warn("No data provided.");

        var csvData = blocks.ConsumeNextBlock();

        var tableName = o.As;
        //remove table if it already exists
        if (queryContext.HasTable(tableName)) queryContext.RemoveTable(tableName);


        try
        {
            if (o.Lines)
            {
                var data = csvData.Split(Environment.NewLine).Select(l => new { Line = l })
                    .ToImmutableArray();
                var table = TableBuilder.CreateFromImmutableData(tableName, data);
                queryContext.AddTable(table);
            }
            else
            {
                var serializer = CsvSerializer.Default(settings, console);
                var source = serializer.LoadFromString(csvData, tableName);
                queryContext.AddTable(source);
            }

            console.Info($"Loaded {NameEscaper.EscapeIfNecessary(tableName)}");
            await context.RunInput(tableName);
        }
        catch (Exception ex)
        {
            console.Warn($"Data malformed: {ex.Message}");
        }
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

        [Option] public bool Lines { get; set; }
    }
}
