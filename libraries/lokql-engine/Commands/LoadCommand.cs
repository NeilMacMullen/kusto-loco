using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

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
        if (!success) exp.Warn($"Unable to load '{o.File}'");
    }

    [Verb("load", aliases: ["ld"],
        HelpText = "loads a data file")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        public string File { get; set; } = string.Empty;

        [Value(1, HelpText = "Name of table (defaults to name of file)")]
        public string As { get; set; } = string.Empty;

        [Option('f', "force", HelpText = "Force reload")]
        public bool Force { get; set; }
    }
}
