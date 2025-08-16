using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
/// Retrieves a named result
/// </summary>
public static class ResultsCommand
{
    internal static  Task RunAsync(CommandContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var res = exp._resultHistory.List();
        var str =  Tabulator.Tabulate(res, "Name|Time|Rows|Columns",s=>s.Name, s => s.Timestamp.ToShortTimeString(),
            s=>s.Result.RowCount.ToString(),s=>s.Result.ColumnCount.ToString());
        exp.Info(str);
        return Task.CompletedTask;
    }

    [Verb("results", HelpText = "shows the list of stored results")]
    internal class Options
    {
       
    }
}
