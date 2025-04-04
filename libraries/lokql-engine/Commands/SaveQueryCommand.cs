using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class SaveQueryCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var queryFolder = exp.Settings.Get(LokqlSettings.QueryPath);
        var filename = InteractiveTableExplorer.ToFullPath(o.File, queryFolder, ".csl");
        exp.Info($"Saving query as '{filename}'");
        var q = exp.GetPreviousResult().Query;
        if (!o.NoSplit)
            q = q
                    .Tokenize("|")
                    .JoinString($"{Environment.NewLine}| ")
                    .Tokenize(";")
                    .JoinString($";{Environment.NewLine}")
                ;

        var text = $@"//{o.Comment}
    {q}
    ";
        await File.WriteAllTextAsync(filename, text);
    }

    [Verb("savequery", aliases: ["sq"],
        HelpText = "save the previous query to a file so you can reuse it")]
    internal class Options : IFileCommandOption
    {
        [Value(0, HelpText = "Name of queryFile", Required = true)]
        public string File { get; set; } = string.Empty;

        [Option('c', HelpText = "Adds a comment")]
        public string Comment { get; set; } = string.Empty;

        [Option(HelpText = "Prevents query being split into multiple lines at '|' and ';'")]
        public bool NoSplit { get; set; }
    }
}
