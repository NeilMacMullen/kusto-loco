using CommandLine;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class QuickCsvCommand
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
        if (tableName.IsBlank())
        {
            exp.Warn("No table name provided.");
            return Task.CompletedTask;
        }

        //remove table if it already exists
        if (exp.GetCurrentContext().HasTable(tableName))
        {
            if (o.Force)
            {
                exp.GetCurrentContext().RemoveTable(tableName);
            }
            else
            {
                exp.Warn($"Table '{tableName}' already exists.  Use '-f' to force reload");
                return Task.CompletedTask;
            }
        }

        try
        {
            var serializer = CsvSerializer.Default(exp.Settings, exp._outputConsole);
            var source = serializer.LoadFromString(csvData, tableName);
            exp.GetCurrentContext().AddTable(source);
            exp.Info($"Loaded {tableName}");
        }
        catch
        {
            exp.Warn($"Data malformed {tableName}");
        }

        return Task.CompletedTask;
    }

    [Verb("csvdata",
        HelpText = @"loads data from csv-formatted inline data")]
    internal class Options
    {
        [Value(0, HelpText = "Name of table ")]
        public string As { get; set; } = string.Empty;

        [Option('f', "force", HelpText = "Force reload")]
        public bool Force { get; set; }
    }
}
