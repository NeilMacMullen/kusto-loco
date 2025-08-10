using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
///     quickly import inline data
/// </summary>
public static class DropTableCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var tableName = o.Table;
        if (tableName.IsNotBlank())
        {
            if (exp.GetCurrentContext().HasTable(tableName))
            {
                exp.GetCurrentContext().RemoveTable(tableName);
                exp.Warn($"Table '{tableName}' removed");
            }
            else
            {
                exp.Warn("No such table");
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
