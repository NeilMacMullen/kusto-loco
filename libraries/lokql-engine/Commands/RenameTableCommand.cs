using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class RenameTableCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        queryContext.RenameTable(o.Current, o.New);
        return Task.CompletedTask;
    }

    [Verb("rename",
        HelpText = """
                   Renames the named table 
                   """
    )]
    internal class Options
    {
        [Value(0, HelpText = "Current Name")] public string Current { get; set; } = "";
        [Value(1, HelpText = "NewName")] public string New { get; set; } = "";
    }
}
