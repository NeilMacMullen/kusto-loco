// See https://aka.ms/new-console-template for more information

using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using KustoLoco.Rendering.ScottPlot;
using NotNullStrings;

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
    if (res.Error.IsNotBlank())
    {
        Console.WriteLine(res.Error);
    }
    else
    {
        if (res.Visualization.ChartType.IsNotBlank())
        {
            var str = ScottPlotKustoResultRenderer.RenderToSixelWithPad(res, new KustoSettingsProvider(),3);
            Console.WriteLine(str);
        }
    }
}
