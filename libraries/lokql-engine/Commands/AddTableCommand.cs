using CommandLine;
using KustoLoco.FileFormats;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class AddTableCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var blocks = econtext.Sequence;

        if (blocks.Complete)
        {
            exp.Warn("No data provided.");
            return Task.CompletedTask;
        }

        var csvData = blocks.Next();

        var tableName = o.As;
        //remove table if it already exists
        if (exp.GetCurrentContext().HasTable(tableName)) exp.GetCurrentContext().RemoveTable(tableName);

        try
        {
            var serializer = CsvSerializer.Default(exp.Settings, exp._outputConsole);
            var source = serializer.LoadFromString(csvData, tableName);
            exp.GetCurrentContext().AddTable(source);
            exp.Info($"Loaded {NameEscaper.EscapeIfNecessary(tableName)}");
        }
        catch (Exception ex)
        {
            exp.Warn($"Data malformed: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    [Verb("addtable",aliases:["csvdata"],
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
