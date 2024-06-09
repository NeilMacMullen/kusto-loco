using CommandLine;

namespace Lokql.Engine.Commands;

public static class ExitCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp.Warn("Exiting...");
        Environment.Exit(0);
        return Task.CompletedTask;
    }

    [Verb("exit", aliases: ["quit"], HelpText = "Exit application")]
    internal class Options;
}
