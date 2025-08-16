using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Writes to the console
/// </summary>
public static class EchoCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;

        console.Info(o.Text);
        return Task.CompletedTask;
    }

    [Verb("echo", HelpText =
        @"writes the supplied text to the output window.  This can be useful when executing a series of
long-running operations.

Examples:
  echo ""starting long-running query""
")]
    internal class Options
    {
        [Value(0, Required = true)]
        public string Text { get; set; } = string.Empty;
    }
}
