using CommandLine;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class DropTableCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        var tableName = o.Table;
        if (tableName.IsNotBlank())
        {
            if (queryContext.HasTable(tableName))
            {
                queryContext.RemoveTable(tableName);
                console.Warn($"Table '{tableName}' removed");
            }
            else
            {
                console.Warn("No such table");
            }
            GC.Collect();
        }

        return Task.CompletedTask;
    }

    [Verb("drop",
        HelpText = """
                   Removes the named table from the context, freeing up memory.
                   Note that memory may not be released until all stored query
                   results have been removed.
                   """
    )]
    internal class Options
    {
        [Value(0, HelpText = "Name of table")] public string Table { get; set; } = "";
    }
}
