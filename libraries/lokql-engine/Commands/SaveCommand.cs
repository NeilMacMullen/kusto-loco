using CommandLine;
using KustoLoco.Core.Settings;

namespace Lokql.Engine.Commands;

/// <summary>
/// Store the last result to a file
/// </summary>
public static class SaveCommand
{
    internal static async Task RunAsync(CommandContext context, Options o)
    {
        var newLayer = new KustoSettingsProvider();
        if (o.NoHeader)
        {
            newLayer.Set("csv.skipheader", "true");
            newLayer.Set("tsv.skipheader", "true");
            newLayer.Set("txt.skipheader", "true");
        }
        var exp = context.Explorer;
        exp.Settings.AddLayer(newLayer);
       
        await exp._loader.SaveResult(exp.GetResult(o.ResultName), o.File);
        exp.Settings.Pop();
    }

    [Verb("save", aliases: ["sv"], HelpText = @"saves query results to a file
Supported formats: csv, tsv, parquet, json, text
If the path is not absolute, the file is stored in kusto.datapath.
If no result name is specified, the most recent result is saved.
Examples:
  .save c:\temp\data.csv           # Save most recent result to CSV
  .save data.parquet myResult      # Save named result 'myResult' to parquet
  .save output.csv --no-header     # Save without column headers")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        [FileOptions(IncludeStandardFormatterExtensions = true)]
        public string File { get; set; } = string.Empty;
        [Value(1, HelpText = "Name of result (or most recent result if left blank)")]
        public string ResultName { get; set; } = string.Empty;
        [Option('n',HelpText = "Avoid writing headers for csv and text files")]
        public bool NoHeader { get; set; }
    }
}
