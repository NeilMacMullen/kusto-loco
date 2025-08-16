using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class ExitCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        console.Warn("Exiting...");
        Environment.Exit(0);
        return Task.CompletedTask;
    }

    [Verb("exit", aliases: ["quit"], HelpText = "Exit application")]
    internal class Options;
}
