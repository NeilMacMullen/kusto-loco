using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     Load a data file
/// </summary>
public static class LoadCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
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

        var success = await exp._loader.LoadTable(exp.GetCurrentContext(), o.File, tableName);
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
    }
}
