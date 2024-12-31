using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class ListTablesCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var context = exp .GetCurrentContext();
        var tableNames = context.TableNames
            .Select(NameEscaper.EscapeIfNecessary)
            .JoinAsLines();
        exp._outputConsole.WriteLine(tableNames);
        return Task.CompletedTask;
    }

    [Verb("listtables", aliases: ["ls", "alltables", "at"],
        HelpText = "Lists all available tables")]
    internal class Options
    {
    }
}
