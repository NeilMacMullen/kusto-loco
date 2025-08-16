using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     list supported file formats for save/load
/// </summary>
public static class FileFormatsCommand
{
    internal static Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var formats =
            Tabulator.Tabulate(
                exp._loader.GetSupportedAdaptors(),
                "Name|Description|Extensions", f => f.Name,f=>f.Description, f => f.Extensions.JoinString(", "));
        exp.Info(formats);
        return Task.CompletedTask;
    }

    [Verb("fileFormats", aliases: ["fmts"], HelpText = "list supported file formats for save/load")]
    internal class Options
    {
    }
}
