using KustoLoco.Core;
using KustoLoco.ScottPlotRendering;
using NotNullStrings;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;

namespace lokqlDx;


public static class ScottPlotter
{
    public static void Render(WpfPlot plotter, KustoQueryResult result)
    {
        plotter.Reset();
         GenericScottPlotter.Render(plotter.Plot, result);
        GenericScottPlotter.UseDarkMode(plotter.Plot);
        plotter.Plot.Title(result.Visualization.PropertyOr("title", DateTime.UtcNow.ToShortTimeString()));
        plotter.Refresh();
       
    }
}
