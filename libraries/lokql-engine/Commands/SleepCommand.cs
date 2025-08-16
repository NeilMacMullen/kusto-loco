using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class SleepCommand
{
    internal static async Task RunAsync(ICommandContext econtext, Options o)
    {
        econtext.Console.Info("Sleeping...");
        await Task.Delay(TimeSpan.FromSeconds(o.Seconds));
        econtext.Console.Info("Finished.");
    }

    [Verb("sleep", HelpText = @"Sleep for a number of seconds (default is 1)
Primarily used for testing but could be used for a basic slide-show")]
    internal class Options
    {
        [Value(0)] public double Seconds { get; set; } =1;
    }
}
