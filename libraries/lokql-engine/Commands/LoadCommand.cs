using CommandLine;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     Load a data file
/// </summary>
public static class LoadCommand
{
    internal static async Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var tableName = o.As.OrWhenBlank(Path.GetFileNameWithoutExtension(o.File));
        //remove table if it already exists
        if (exp.GetCurrentContext().HasTable(tableName))
        {
            if (o.Force)
            {
                exp.GetCurrentContext().RemoveTable(tableName);
            }
            else
            {
                exp.Warn($"Table '{tableName}' already exists.  Use '.load -f' to force reload");
                return;
            }
        }

        var newLayer = new KustoSettingsProvider();
        newLayer.Set(CsvSerializer.CsvSerializerSettings.TrimCells.Name, !o.NoTrim);
        newLayer.Set(CsvSerializer.CsvSerializerSettings.InferColumnNames.Name, o.NoHeader);
        newLayer.Set(CsvSerializer.CsvSerializerSettings.Separator.Name, o.Separator);
        newLayer.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, o.NoInfer);
        newLayer.Set(ExcelSerializerSettings.SkipTypeInference.Name, o.NoInfer);

        exp.Settings.AddLayer(newLayer);
        var success = await exp._loader.LoadTable(exp.GetCurrentContext(), o.File, tableName);
        exp.Settings.Pop();
        if (!success)
        {
            exp.Warn($"Unable to load '{o.File}'");
        }
        else
        {
            var escapedName = NameEscaper.EscapeIfNecessary(tableName);
            exp.Info($"Table {escapedName} now available");
            await exp.RunInput(escapedName);
        }
    }


    [Verb("load", aliases: ["ld"],
        HelpText = @"loads a data file.  Supported formats are csv, tsv, json, parquet and text.
The table name defaults to the file name.
If the path is not rooted, the file is searched for in path set by kusto.datapath
If the table already exists, it will not be reloaded unless the -f option is used.
When loading text files, a single column named 'Line' is created.
Examples:
 .load c:\temp\data.csv        
 .load d.parquet data2 ")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        [FileOptions(IncludeStandardFormatterExtensions = true)]
        public string File { get; set; } = string.Empty;

        [Value(1, HelpText = "Name of table (defaults to name of file)")]
        public string As { get; set; } = string.Empty;

        [Option('f', "force", HelpText = "Force reload")]
        public bool Force { get; set; }

        [Option('n', "noheader", HelpText = "Assume no header row when loading csv/tsv files")]
        public bool NoHeader { get; set; }

        [Option('s', "separator", HelpText = "Separator character for csv files")]
        public string Separator { get; set; } = ",";

        [Option('t', "notrim", HelpText = "Skip trim of cells for csv and tsv")]
        public bool NoTrim { get; set; }

        [Option("noInfer", HelpText = "leave all columns as strings for csv/tsv")]
        public bool NoInfer { get; set; }
    }
}
