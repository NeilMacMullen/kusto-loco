using CommandLine;

namespace Lokql.Engine.Commands;

public static class ShowCommand
{
    internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
    {
        exp.ShowResultsToConsole(exp._prevResult, o.Offset, o.NumToShow);
        await Task.CompletedTask;
    }

    [Verb("show", HelpText = "show the last results again")]
    internal class Options
    {
        [Value(0, HelpText = "Rows to show")] public int NumToShow { get; set; } = 50;


        [Value(1, HelpText = "Offset to show data from")]
        public int Offset { get; set; } = 0;
    }
}
