using CommandLine;

namespace Lokql.Engine.Commands;

public static class SleepCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        econtext.Explorer.Info("Sleeping...");
        await Task.Delay(TimeSpan.FromSeconds(o.Seconds));
    }

    [Verb("sleep", HelpText = "render last results as html and save in active report")]
    internal class Options
    {
        [Value(0)] public double Seconds { get; set; } =0;
    }
}
