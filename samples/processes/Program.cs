using System.Reflection;
using CliRendering;
using KustoLoco.Core;
using KustoLoco.Rendering;
using KustoLoco.Rendering.ScottPlot;
using NLog.LayoutRenderers;
using NotNullStrings;
using Spectre.Console;

//validate arguments
ShowHelpIfAppropriate(false);
var renderingPreference = CliRenderer.GetValidatedRenderingPreference(args, 2);
if (renderingPreference.IsBlank())
        ShowHelpIfAppropriate(true);
var renderer = new CliRenderer(renderingPreference);

var query = "processes |" +args.Last();  //add the table name to make querying easier

//get the proceseses, turn them into a table and then run the query
var processes = ProcessReader.GetProcesses();
var context = new KustoQueryContext();
context.WrapDataIntoTable("processes", processes);
var result = await context.RunQuery(query);

//display the results
renderer.DisplayQueryResult(result);

return;


void ShowHelpIfAppropriate(bool force)
{
    if (!force && (args.Length is > 0 and < 3)) return;

    var programName = $"{Assembly.GetExecutingAssembly().GetName().Name}.exe";
    var help = $"""
                This program demonstrates the use of KQL to query the current process list.
                A few examples:
                 {programName} "take 5"
                 {programName} "where Name contains 'dotnet'"
                 {programName} "summarize TotalThreads=sum(NumThreads) by Name | order by TotalThreads | take 10"
                 {programName} png "summarize Instances=count() by Name | order by Instances | take 10 | render piechart with (title='process hogs')"
                 
                 If two arguments are specified the first is interpreted as directive to control chart rendering.
                   
                """;
    help += CliRenderer.PreferencesHelp;
    AnsiConsole.MarkupLineInterpolated($"[yellow]{help}[/]");
    Environment.Exit(0);
}
