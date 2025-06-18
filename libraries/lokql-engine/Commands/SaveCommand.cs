using CommandLine;
using KustoLoco.Core.Settings;

namespace Lokql.Engine.Commands;

/// <summary>
/// Save the last result to a file
/// </summary>
public static class SaveCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var newLayer = new KustoSettingsProvider();
        if (o.NoHeader)
        {
            newLayer.Set("csv.skipheader", "true");
            newLayer.Set("tsv.skipheader", "true");
        }
        var exp = econtext.Explorer;
        exp.Settings.AddLayer(newLayer);
       
        await exp._loader.SaveResult(exp.GetResult(o.ResultName), o.File);
        exp.Settings.Pop();
    }

    [Verb("save", aliases: ["sv"], HelpText = @"save results to file.
Supported formats are csv,tsv,parquet.json,text
If the path is not rooted, the file is stored in kusto.datapath
If the name of the result is not specified, the most recent result is saved.
Examples:
  .save c:\temp\data.csv #saves the most recent result to a csv file
  .save d.parquet abc    #saves a named result called 'abc' to a parquet file
")]
    internal class Options : IFileCommandOption
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        public string File { get; set; } = string.Empty;
        [Value(1, HelpText = "Name of result (or most recent result if left blank)")]
        public string ResultName { get; set; } = string.Empty;
        [Option(HelpText = "Avoid writing headers for csv and text files")]
        public bool NoHeader { get; set; }
    }
}
