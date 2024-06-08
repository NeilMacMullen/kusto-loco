using CommandLine;

namespace Lokql.Engine.Commands;

public static class FormatCommand
{
    internal static void Run(InteractiveTableExplorer exp, Options o)
    {
        if (o.Max > 0)
            exp._currentDisplayOptions = exp._currentDisplayOptions with { MaxToDisplay = o.Max };
        exp.Info($"Set: {exp._currentDisplayOptions}");
    }

    [Verb("display", aliases: ["d"], HelpText = "change the output format")]
    internal class Options
    {
        [Option('m', HelpText = "Maximum number of items to display in console")]
        public int Max { get; set; } = -1;
    }
}
