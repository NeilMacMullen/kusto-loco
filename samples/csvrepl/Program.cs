using System.Reflection;
using CliRendering;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using KustoLoco.Rendering.ScottPlot;
using NotNullStrings;
using Spectre.Console;

ShowHelpIfAppropriate(false);
var renderingPreference = CliRenderer.GetValidatedRenderingPreference(args, 2);
if (renderingPreference.IsBlank())
    ShowHelpIfAppropriate(true);
var renderer = new CliRenderer(renderingPreference);


var result = await CsvSerializer.Default(new KustoSettingsProvider(), new SystemConsole())
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
    var query = "data | " + Console.ReadLine()!.Trim();
    var res = await context.RunQuery(query);
    renderer.DisplayQueryResult(res);
}


void ShowHelpIfAppropriate(bool force)
{
    if (!force && (args.Length is > 0 and < 3)) return;
    var programName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
    var help = $"""
                This program demonstrates the use of KQL to query a CSV file specified by the user.
                Usage:
                 {programName} c:\temp\mydata.csv
                 
                If two arguments are specified the first is interpreted as directive to control chart rendering.
                 
                """;
    help +=CliRenderer.PreferencesHelp;
    AnsiConsole.MarkupLineInterpolated($"[yellow]{help}[/]");
    Environment.Exit(0);
}

