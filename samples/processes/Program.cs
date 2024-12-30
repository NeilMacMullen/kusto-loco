using System.Reflection;
using KustoLoco.Core;
using KustoLoco.Rendering;
using NotNullStrings;
using Spectre.Console;

ShowHelpIfAppropriate();


var processes = ProcessReader.GetProcesses();
var query = args.First();
var context = new KustoQueryContext();
context.WrapDataIntoTable("processes", processes);
var result = await context.RunQuery(query);

DisplayQueryResult(result);

//end of program


void DisplayQueryResult(KustoQueryResult queryResult)
{
    // if we got an error display it and skip rendering an empty table
    if (queryResult.Error.IsNotBlank())
    {
        AnsiConsole.MarkupLineInterpolated($"[red]{queryResult.Error}[/]");
        return;
    }

    //display the results as a pretty Spectre.Console table
    var table = new Table();

    // Add columns with header names
    foreach (var column in queryResult.ColumnDefinitions()) table.AddColumn(column.Name);

    // Add rows.  Note that cells could contain nulls in the general case
    foreach (var row in queryResult.EnumerateRows())
    {
        var rowCells = row.Select(CellToString).ToArray();
        table.AddRow(rowCells);
    }

    AnsiConsole.Write(table);

    //render the results as a chart if we were asked to do that
    KustoResultRenderer.RenderChartInBrowser(queryResult);

    return;

    string CellToString(object? cell)
    {
        return cell?.ToString() ?? "<null>";
    }
}


void ShowHelpIfAppropriate()
{
    if (args.Length != 0) return;

    var programName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
    var help = $"""
                This program demonstrates the use of KQL to query the current process list.
                A few examples:
                 {programName} "take 5"
                 {programName} "where Name contains 'dotnet'"
                 {programName} "summarize TotalThreads=sum(NumThreads) by Name | order by TotalThreads | take 10"
                 {programName} "summarize Instances=count() by Name | order by Instances | take 10 | render piechart with (title='process hogs')"
                """;
    AnsiConsole.MarkupLineInterpolated($"[yellow]{help}[/]");
    Environment.Exit(0);
}
