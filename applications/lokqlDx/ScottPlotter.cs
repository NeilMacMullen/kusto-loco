using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using NotNullStrings;
using ScottPlot;
using ScottPlot.WPF;

namespace lokqlDx;

public static class ScottPlotter
{
    public static async Task<bool> Render(WpfPlot plotter, KustoQueryResult result)
    {
        plotter.Reset();
        var r = await Render(plotter.Plot, result);
        UseDarkMode(plotter.Plot);
        plotter.Refresh();
        return r;
    }

    private static void UseDarkMode(Plot plot)
    {
        plot.Add.Palette = new ScottPlot.Palettes.Penumbra();
        plot.FigureBackground.Color = Color.FromHex("#181818");
        plot.DataBackground.Color = Color.FromHex("#1f1f1f");

        // change axis and grid colors
        plot.Axes.Color(Color.FromHex("#d7d7d7"));
        plot.Grid.MajorLineColor = Color.FromHex("#404040");

        // change legend colors
        plot.Legend.BackgroundColor = Color.FromHex("#404040");
        plot.Legend.FontColor = Color.FromHex("#d7d7d7");
        plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");

        plot.Legend.FontSize = 16;
        plot.ShowLegend(Edge.Right);
    }

    public static async Task<bool> Render(Plot plot, KustoQueryResult result)
    {
        plot.Clear();
       

        if (result.Visualization.ChartType.IsNotBlank() && result.ColumnCount == 3)
        {
            var series = result.EnumerateRows()
                .GroupBy(r => r[2])
                .ToArray();
            foreach (var s in series)
            {
                var xData = s.Select(r => r[0]).ToArray();
                var yData = s.Select(r => r[1]).ToArray();
                var legend = s.Key?.ToString() ?? string.Empty;
                AddSeries(result.Visualization, plot, xData, yData, legend);
            }

            if (result.ColumnDefinitions()[0].UnderlyingType == typeof(DateTime))
                plot.Axes.DateTimeTicksBottom();

            return true;
        }

        return await Task.FromResult(true);
    }

    private static void AddSeries(VisualizationState state, Plot plot, object?[] xData, object?[] yData,
        string legend)
    {
        switch (state.ChartType.ToLowerInvariant())
        {
            case "linechart":
            {
                var line = plot.Add.Scatter(xData, yData);
                line.LegendText = legend;
                break;
            }
            case "scatterchart":
            {
                var line = plot.Add.ScatterPoints(xData, yData);

                line.LegendText = legend;
                break;
            }
            case "barchart":
            {
                var bars = xData.Zip(yData).Select(t =>
                    new Bar()
                    {
                        Position = ((DateTime)t.First!).ToOADate(),
                        Value = (double)t.Second!
                    }
                ).ToArray();
                plot.Add.Bars(bars);
                break;
            }
            case "ladderchart":
            case "heatmap":
                break;
        }
    }
}
