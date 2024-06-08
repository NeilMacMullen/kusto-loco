using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class FileFormatsCommand
{
    internal static Task RunAsync(InteractiveTableExplorer exp, Options o)
    {
        var formats = exp._loader.GetSupportedAdaptors()
            .Select(f => $"{f.Name} ({f.Extensions.JoinString(", ")})")
            .JoinAsLines();
        exp.Info(formats);
        return Task.CompletedTask;
    }

    [Verb("formats", aliases: ["fmts"], HelpText = "list supported file formats for save/load")]
    internal class Options
    {
    }
}
