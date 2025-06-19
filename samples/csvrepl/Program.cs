using System.Reflection;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using KustoLoco.Rendering.ScottPlot;
using NotNullStrings;
using Spectre.Console;

ShowHelpIfAppropriate();
var settings = new KustoSettingsProvider();
var result = await CsvSerializer.Default(settings, new SystemConsole())
    .LoadTable(args.First(), "data");

if (result.Error.IsNotBlank())
{
    Console.WriteLine(result.Error);
    return;
}

var context = new KustoQueryContext()
    .AddTable(result.Table);

while (true)
{
    Console.Write(">");
    var query = Console.ReadLine()!.Trim();
    var res = await context.RunQuery(query);
    DisplayQueryResult(res);
}


void ShowHelpIfAppropriate()
{
    if (args.Length != 0) return;
    var programName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
    var help = $"""
                This program demonstrates the use of KQL to query a CSV file specified by the user.
                Usage:
                 {programName} c:\temp\mydata.csv
                Note that you must use Windows Terminal Preview in order to correctly display charts     
                """;
    AnsiConsole.MarkupLineInterpolated($"[yellow]{help}[/]");
    Environment.Exit(0);
}

void DisplayQueryResult(KustoQueryResult queryResult)
{
    // if we got an error display it and skip rendering an empty table
    if (queryResult.Error.IsNotBlank())
    {
        AnsiConsole.MarkupLineInterpolated($"[red]{queryResult.Error}[/]");
        return;
    }

    if (queryResult.IsChart)
    {
            var str = ScottPlotKustoResultRenderer.RenderToSixelWithPad(queryResult, new KustoSettingsProvider(), 3);
            Console.WriteLine(str);
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

    return;

    string CellToString(object? cell)
    {
        return cell?.ToString() ?? "<null>";
    }
}
