// See https://aka.ms/new-console-template for more information
using Microsoft.VisualBasic;
using System.IO.Pipelines;
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
            using var plot = new ScottPlot.Plot();
            ScottPlotKustoResultRenderer.RenderToPlot(plot, res, settings);
            var w = Console.WindowWidth;
            var h = Console.WindowHeight;
       
            var (a, b) = TermHelper.GetCellSize();
            var bytes = plot.GetImageBytes(w*a, h*b);
            var src = ImageSource.FromBytes(bytes);
            var q = Octree.Quantize(src, 250);
            Console.WriteLine(SixelMaker.FrameToSixelString(q));
          
        }
    }
}

//from https://github.com/bacowan/cSharpColourQuantization/blob/master/ColourQuantization/Octree.cs

public static class TermHelper
{
    public static (int w,int h) GetCellSize()
    {

        var response = GetControlSequenceResponse("[16t");

        try
        {
            var parts = response.Split(';', 't');
            var PixelWidth = int.Parse(parts[2]);
            var PixelHeight = int.Parse(parts[1]);
           return (PixelWidth, PixelHeight);
        }
        catch
        {
            Console.WriteLine("no cell size");
        }
        return (1, 1);

    }
    public static string GetControlSequenceResponse(string controlSequence)
    {
        char? c;
        var response = string.Empty;

        // Console.Write($"{Constants.ESC}{controlSequence}{Constants.ST}");
        Console.Write($"{ESC}{controlSequence}");
        do
        {
            c = Console.ReadKey(true).KeyChar;
            response += c;
        } while (c != 'c' && Console.KeyAvailable);

        return response;
    }

    internal const string ESC = "\u001b";

}
