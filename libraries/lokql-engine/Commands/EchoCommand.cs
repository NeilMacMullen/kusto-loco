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
        @"writes text to the output console
Useful for logging progress in scripts or during long-running operations.
Examples:
  .echo Starting data analysis
  .echo Loading tables...  
")]
    internal class Options
    {
        [Value(0, Required = true)]
        public string Text { get; set; } = string.Empty;
    }
}
