using CommandLine;
using KustoLoco.FileFormats;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class CsvDataCommand
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

    [Verb("csvdata",
        HelpText = @"loads data from csv-formatted inline text
Examples:
  .csvdata ages
  Name,Age
  Alice,30
  Bob,28")]
    internal class Options
    {
        [Value(0, HelpText = "Name of table (defaults to 'csvdata')")]
        public string As { get; set; } = "csvdata";
    }
}
