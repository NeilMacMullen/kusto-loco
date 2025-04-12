using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using ScottPlot.WPF;

namespace lokqlDx;

public static class ScottPlotter
{
    public static void Render(WpfPlot plotter, KustoQueryResult result, KustoSettingsProvider settings)
    {
        plotter.Reset();
        ScottPlotKustoResultRenderer.RenderToPlot(plotter.Plot, result,settings);
      
        
        plotter.Refresh();
    }
}
