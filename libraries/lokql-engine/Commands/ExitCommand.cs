using CommandLine;

namespace Lokql.Engine.Commands;

public static class ExitCommand
{
    internal static void Run(InteractiveTableExplorer exp, Options o)
    {
        exp.Warn("Exiting...");
        Environment.Exit(0);
    }

    [Verb("exit", aliases: ["quit"], HelpText = "Exit application")]
    internal class Options;
}
