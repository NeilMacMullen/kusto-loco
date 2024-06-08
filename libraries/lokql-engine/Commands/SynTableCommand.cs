using CommandLine;

namespace Lokql.Engine.Commands;

public static class SynTableCommand
{
    internal static void Run(InteractiveTableExplorer exp, Options o)
    {
        exp.Info($"Creating synonym for table {o.CurrentName} as {o.As} ...");

        exp.GetCurrentContext().ShareTable(o.CurrentName, o.As);
    }

    [Verb("synonym", aliases: ["syn", "alias"], HelpText = "provides a synonym for a table")]
    internal class Options
    {
        [Value(0, HelpText = "table name", Required = true)]
        public string CurrentName { get; set; } = string.Empty;

        [Value(1, HelpText = "Synonym", Required = true)]
        public string As { get; set; } = string.Empty;
    }
}
