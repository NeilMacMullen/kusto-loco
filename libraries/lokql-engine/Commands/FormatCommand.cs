using CommandLine;

namespace Lokql.Engine.Commands;

public static class FormatCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        if (o.Max > 0)
            exp._currentDisplayOptions = exp._currentDisplayOptions with { MaxToDisplay = o.Max };
        exp.Info($"Set: {exp._currentDisplayOptions}");
        return Task.CompletedTask;
    }

    [Verb("display", aliases: ["d"], HelpText = "change the output format")]
    internal class Options
    {
        [Option('m', HelpText = "Maximum number of items to display in console")]
        public int Max { get; set; } = -1;
    }
}
