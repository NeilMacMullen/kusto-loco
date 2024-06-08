using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class AllTablesCommand
{
    internal static void Run(InteractiveTableExplorer exp, Options o)
    {
        var context = exp.GetCurrentContext();
        var tableNames = context.TableNames
            .Select(t => $"['{t}']")
            .JoinAsLines();
        exp._outputConsole.WriteLine(tableNames);
    }

    [Verb("listtables", aliases: ["ls", "alltables", "at"],
        HelpText = "Lists all available tables in the global context")]
    internal class Options
    {
    }
}
