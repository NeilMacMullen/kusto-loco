using CommandLine;

namespace Lokql.Engine.Commands;

public static class RunScriptCommand
{
    internal static async Task RunAsync(CommandContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var scriptFolder = exp.Settings.Get(LokqlSettings.ScriptPath);

        var filename = InteractiveTableExplorer.ToFullPath(o.File, scriptFolder, ".dfr");

        exp.Info($"Loading script '{filename}'..");
        var text = await File.ReadAllTextAsync(filename);
        await exp.RunInput(text);
    }

    [Verb("run", aliases: ["script", "r"],
        HelpText = "run a script")]
    internal class Options
    {
        [Value(0, HelpText = "Name of script", Required = true)]
        [FileOptions(Extensions = [".dfr"])]
        public string File { get; set; } = string.Empty;
    }
}
