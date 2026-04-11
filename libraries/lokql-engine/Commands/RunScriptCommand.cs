using CommandLine;

namespace Lokql.Engine.Commands;

public static class RunScriptCommand
{
    internal static async Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var scriptFolder = exp.Settings.Get(LokqlSettings.ScriptPath);

        var filename = InteractiveTableExplorer.ToFullPath(o.File, scriptFolder, ".dfr");

        exp.Info($"Loading script '{filename}'..");
        var text = await File.ReadAllTextAsync(filename);
        await exp.RunInput(text);
    }

    [Verb("run", aliases: ["script", "r"],
        HelpText = @"executes a script file containing KQL queries and commands
Script files should have .dfr extension.
If path is not absolute, searches in lokql.scriptpath directory.
Examples:
  .run analysis.dfr       # Run script from script path
  .run C:\scripts\load.dfr # Run from absolute path")]
    internal class Options
    {
        [Value(0, HelpText = "Name of script", Required = true)]
        [FileOptions(Extensions = [".dfr"])]
        public string File { get; set; } = string.Empty;
    }
}
