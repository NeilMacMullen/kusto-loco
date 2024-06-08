using CommandLine;

namespace Lokql.Engine.Commands;

public static class MaterializeCommand
{
    internal static void Run(InteractiveTableExplorer exp, Options o)
    {
        exp.GetCurrentContext().MaterializeResultAsTable(exp._prevResult, o.As);
        exp.Info($"Table '{o.As}' now available");
    }

    [Verb("materialize", aliases: ["materialise", "mat"],
        HelpText = "save last results back into context as a table")]
    internal class Options
    {
        [Value(0, Required = true, HelpText = "Name of table")]
        public string As { get; set; } = string.Empty;
    }
}
