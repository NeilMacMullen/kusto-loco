using CommandLine;

namespace Lokql.Engine.Commands;

public static class QueryCommand
{
    internal static async Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var queryFolder = exp.Settings.Get(LokqlSettings.QueryPath);
        var filename = InteractiveTableExplorer.ToFullPath(o.File, queryFolder, ".csl");
        exp.Info($"Fetching query '{filename}'");
        var query = await File.ReadAllTextAsync(filename);
        await exp.RunInput($"{o.Prefix}{query}");
    }

    [Verb("query", aliases: ["q"],
        HelpText = "run a multi-line query from a file")]
    internal class Options
    {
        [Value(0, HelpText = "Name of queryFile", Required = true)]
        [FileOptions(Extensions = [".csl"])]
        public string File { get; set; } = string.Empty;

        [Option('p', HelpText = "prefix to add to script before running")]
        public string Prefix { get; set; } = string.Empty;
    }
}
