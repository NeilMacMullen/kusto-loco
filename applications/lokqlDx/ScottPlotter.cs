using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using NotNullStrings;
using ScottPlot;
using ScottPlot.WPF;

namespace lokqlDx;

public interface IAxisLookup
{
    public double ValueFor(object? o);
    string GetLabel(double position);
    Dictionary<double, string> Dict();
}

public class StringAxisLookup : IAxisLookup
{
    private readonly Dictionary<object, double> _lookup;
    private readonly Dictionary<double, object> _labelLookup;

    public double ValueFor(object? o) => o is null ? 0 : _lookup[o];

    public string GetLabel(double position) => _labelLookup.TryGetValue(position, out var o)
        ? o.ToString().NullToEmpty()
        : string.Empty;

    public Dictionary<double, string> Dict() =>
        _labelLookup.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString().NullToEmpty()!);

    public StringAxisLookup(Dictionary<object, double> lookup)
    {
        _lookup = lookup;
        _labelLookup = _lookup.ToDictionary(kv => kv.Value, kv => kv.Key);
    }
}

public class DateTimeAxisLookup : IAxisLookup
{
    public double ValueFor(object? o) => o is null ? 0 : ((DateTime)o).ToOADate();
    public string GetLabel(double position) => throw new NotImplementedException();
    public Dictionary<double, string> Dict() => throw new NotImplementedException();
}

public class DoubleAxisLookup : IAxisLookup
{
    public double ValueFor(object? o) => o is null ? 0 : (double)o;
    public string GetLabel(double position) => throw new NotImplementedException();
    public Dictionary<double, string> Dict() => throw new NotImplementedException();
}

public class AxisLookup
{
    public static IAxisLookup From(ColumnResult col, object?[] data)
    {
        if (col.UnderlyingType == typeof(string))
        {
            var d = new Dictionary<object, double>();
            var index = 1.0;
            foreach (var o in data)
            {
                if (o is null)
                    continue;
                if (!d.ContainsKey(o))
                    d[o] = index++;
            }

            return new StringAxisLookup(d);
        }

        if (col.UnderlyingType == typeof(DateTime))
        {
            return new DateTimeAxisLookup();
        }

        if (col.UnderlyingType == typeof(double))
        {
            return new DoubleAxisLookup();
        }

        throw new InvalidOperationException("Unsupported type");
    }
}

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

            var xColumn = result.ColumnDefinitions()[0];
            var fullXdata = result.EnumerateColumnData(xColumn).ToArray();

            var lookup = AxisLookup.From(xColumn, fullXdata);
            foreach (var s in series)
            {
                var xData = s.Select(r => r[0]).ToArray();
                var yData = s.Select(r => r[1]).ToArray();
                var legend = s.Key?.ToString() ?? string.Empty;
                AddSeries(result.Visualization, plot, xData, yData, legend, lookup);
            }

            if (xColumn.UnderlyingType == typeof(DateTime))
                plot.Axes.DateTimeTicksBottom();

            if (xColumn.UnderlyingType == typeof(string))
            {
                Tick[] ticks =
                    lookup.Dict().Select(kv =>
                        new Tick(kv.Key, kv.Value)).ToArray();


                if (result.Visualization.ChartType.Contains("barchart"))
                {
                    plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                    plot.Axes.Left.MajorTickStyle.Length = 0;
                    plot.HideGrid();


                    plot.Axes.Margins(left: 0);
                }
                else
                {
                    plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
                    plot.Axes.Bottom.MajorTickStyle.Length = 0;
                    plot.HideGrid();
                    plot.Axes.Margins(bottom: 0);
                }
            }


            return true;
        }

        return await Task.FromResult(true);
    }

    private static void AddSeries(VisualizationState state, Plot plot, object?[] xData, object?[] yData,
        string legend, IAxisLookup xLookup)
    {
        switch (state.ChartType.ToLowerInvariant())
        {
            case "linechart":
            {
                var xpoints = xData.Select(xLookup.ValueFor).ToArray();
                var line = plot.Add.Scatter(xpoints, yData);
                line.LegendText = legend;
                break;
            }
            case "scatterchart":
            {
                var xpoints = xData.Select(xLookup.ValueFor).ToArray();
                var line = plot.Add.ScatterPoints(xpoints, yData);

                line.LegendText = legend;
                break;
            }
            case "barchart":
            {
                var xpoints = xData.Select(xLookup.ValueFor).ToArray();
                var b = plot.Add.Bars(xpoints, yData);
                b.LegendText = legend;
                b.Horizontal = true;
                break;
            }
            case "columnchart":
            {
                var xpoints = xData.Select(xLookup.ValueFor).ToArray();
                var b = plot.Add.Bars(xpoints, yData);
                b.LegendText = legend;
                break;
            }
            case "ladderchart":
            case "heatmap":
                break;
        }
    }
}
