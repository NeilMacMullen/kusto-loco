using System.Reflection;
using KustoLoco.Core;
using KustoLoco.FileFormats;
using NotNullStrings;
using Spectre.Console;

ShowHelpIfAppropriate();
var settings = new KustoSettings();
var result = await CsvSerializer.Default
    .LoadTable(args.First(), "data", new ConsoleProgressReporter(),settings);

if (result.Error.IsNotBlank())
{
    Console.WriteLine(result.Error);
    return;
}

var context = new KustoQueryContext();
context.AddTable(result.Table);

while (true)
{
    Console.Write(">");
    var query = Console.ReadLine().Trim();
    var res = await context.RunQuery(query);
    if (res.Error.IsNotBlank())
    {
        Console.WriteLine(res.Error);
    }
    else
    {
        Console.WriteLine(KustoFormatter.Tabulate(res));
        Console.WriteLine($"{res.QueryDuration}ms");
    }
}


void ShowHelpIfAppropriate()
{
    if (args.Length != 0) return;
    var programName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
    var help = $"""
                This program demonstrates the use of KQL to query a CSV file specified by the user.
                Usage:
                 {programName} c:\temp\mydata.csv
                """;
    AnsiConsole.MarkupLineInterpolated($"[yellow]{help}[/]");
    Environment.Exit(0);
}
