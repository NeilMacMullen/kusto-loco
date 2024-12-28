using CommandLine;

namespace Lokql.Engine.Commands;

public static class MaterializeCommand
{
    internal static Task Run(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp.GetCurrentContext().MaterializeResultAsTable(exp.GetPreviousResult(), o.As);
        exp.Info($"Table '{o.As}' now available");
        return Task.CompletedTask;
    }

    [Verb("materialize", aliases: ["materialise", "mat"],
        HelpText = "save last results back into context as a table")]
    internal class Options
    {
        [Value(0, Required = true, HelpText = "Name of table")]
        public string As { get; set; } = string.Empty;
    }
}
