using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
/// Retrieves a named result
/// </summary>
public static class ResultsCommand
{
    internal static  Task RunAsync(CommandContext context, Options o)
    {
        var exp = context.Explorer;
        var res = exp._resultHistory.List();
        var str =  Tabulator.Tabulate(res, "Name|Time|Rows|Columns",s=>s.Name, s => s.Timestamp.ToShortTimeString(),
            s=>s.Result.RowCount.ToString(),s=>s.Result.ColumnCount.ToString());
        exp.Info(str);
        return Task.CompletedTask;
    }

    [Verb("results", HelpText = @"displays a list of all stored query results
Shows result name, timestamp, row count, and column count.
Use with .materialize to convert a stored result back to a table.
Example:
  .results           # List all stored query results")]
    internal class Options
    {
       
    }
}
