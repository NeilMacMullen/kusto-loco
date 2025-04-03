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

public static class GenericScottPlotter
{
    private const string StandardAxisPrefs= "tno|nno|noo|tn|nn|on|to";


    public static void UseDarkMode(Plot plot)
    {
        
        plot.Add.Palette = new Penumbra();
        plot.FigureBackground.Color = Color.FromHex("#181818");
        plot.DataBackground.Color = Color.FromHex("#1f1f1f");

        // change axis and grid colors
        //plot.Axes.Color(Color.FromHex("#d7d7d7"));
        plot.Axes.Color(Color.FromHex("#ffffff"));
        plot.Grid.MajorLineColor = Color.FromHex("#404040");

        // change legend colors
        plot.Legend.BackgroundColor = Color.FromHex("#404040");
        plot.Legend.FontColor = Color.FromHex("#d7d7d7");
        plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");

        plot.Legend.FontSize = 16;
        plot.ShowLegend(Edge.Right);
    }


    public static void Render(Plot plot, KustoQueryResult result)
    {
        var accessor = new ResultChartAccessor(result);
        plot.Clear();
        plot.Add.Palette = new Penumbra();
        if (accessor.Kind() == ResultChartAccessor.ChartKind.Pie && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor,"on|ot",0,1,0);

            var slices = accessor.CalculateSeries()
                .Select(ser => new PieSlice
                {
                    Label = ser.Legend,
                    LegendText = ser.Legend,
                    Value = ser.Y[0],
                    FillColor = plot.Add.Palette.GetColor(ser.Index)
                })
                .ToArray();
            plot.Add.Pie(slices);
            plot.ShowLegend();
            // hide unnecessary plot components
            plot.Axes.Frameless();
            plot.HideGrid();
           
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Line && result.ColumnCount >=2)
        {
           
            StandardAxisAssignment(accessor, StandardAxisPrefs, 0, 1, 2);
            foreach (var ser in accessor.CalculateSeries())
            {
                var line = plot.Add.Scatter(ser.X, ser.Y);
                line.LegendText = ser.Legend;
            }

            FixupAxisTicks(plot, accessor, false);
            return ;
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Scatter && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPrefs,0, 1, 2);
            foreach (var ser in accessor.CalculateSeries())
            {
                var line = plot.Add.ScatterPoints(ser.X, ser.Y);
                line.LegendText = ser.Legend;
            }

            FixupAxisTicks(plot, accessor, false);
            return ;
        }


        if (accessor.Kind() == ResultChartAccessor.ChartKind.Column && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPrefs, 0, 1, 2);
            var acc = accessor.CreateAccumulatorForStacking();
            var barWidth = accessor.GetSuggestedBarWidth();
            var bars = accessor.CalculateSeries()
                .Select(ser => CreateBars(plot, acc, ser, false, barWidth))
                .ToArray();

            FixupAxisTicks(plot, accessor, false);
            return ;
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Bar && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPrefs, 0, 1, 2);
            var acc = accessor.CreateAccumulatorForStacking();
            var barWidth = accessor.GetSuggestedBarWidth();
            var bars = accessor.CalculateSeries()
                .Select(ser => CreateBars(plot, acc, ser, true, barWidth))
                .ToArray();

            FixupAxisTicks(plot, accessor, true);
            return;
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Ladder && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, "tto|nno", 0, 1, 2);

            var bars = accessor.CalculateSeries()
                .Select(ser => CreateRangeBars(plot, ser, true))
                .ToArray();
            FixupAxisForLadder(plot, accessor);
            return;
        }

        return ;
    }

    private static void StandardAxisAssignment(ResultChartAccessor accessor,

        string preferences,int x,int y,int s)
    {
        var cols = accessor.TryOrdering(preferences);
        accessor.AssignXColumn(cols[x].Index);
        accessor.AssignValueColumn(cols[y].Index);
        if (s< cols.Length)
            accessor.AssignSeriesNameColumn(cols[s].Index);
    }


    private static void SetTicksOnAxis(IAxis axis, GenericTick[] ticks)
    {
        axis.TickGenerator = new NumericManual(ticks.ToMajorTicks());
        axis.MajorTickStyle.Length = 0;
    }


    private static void FixupAxisTicks(Plot plot, ResultChartAccessor accessor, bool invertAxes)
    {
        if (invertAxes)
        {
            if (accessor.XisDateTime)
                MakeYAxisDateTime(plot);
            if (accessor.YisDateTime)
                plot.Axes.DateTimeTicksBottom();
            if (accessor.XisNominal)
            {
                var ticks = accessor.GetXTicks();
                SetTicksOnAxis(plot.Axes.Left, ticks);
                plot.Axes.Margins(left: 0);
            }

            if (accessor.YisNominal)
            {
                var ticks = accessor.GetYTicks();
                SetTicksOnAxis(plot.Axes.Bottom, ticks);
                plot.Axes.Margins(bottom: 0);
            }
            plot.Axes.Left.Label.Text = accessor.GetXLabel();
            plot.Axes.Bottom.Label.Text = accessor.GetYLabel();
        }
        else
        {
            if (accessor.XisDateTime)
                plot.Axes.DateTimeTicksBottom();

            if (accessor.XisNominal)
            {
                var ticks = accessor.GetXTicks();
                SetTicksOnAxis(plot.Axes.Bottom, ticks);
                plot.Axes.Margins(bottom: 0);
            }

            if (accessor.YisDateTime)
                MakeYAxisDateTime(plot);

            if (accessor.YisNominal)
            {
                var ticks = accessor.GetYTicks();
                SetTicksOnAxis(plot.Axes.Left, ticks);
                plot.Axes.Margins(left: 0);
            }

            plot.Axes.Left.Label.Text = accessor.GetYLabel();
            plot.Axes.Bottom.Label.Text = accessor.GetXLabel();
        }

        if (accessor.XisNominal || accessor.YisNominal) plot.HideGrid();
    }

    private static void MakeYAxisDateTime(Plot plot) => plot.Axes.Left.TickGenerator = new DateTimeAutomatic();

    private static void FixupAxisForLadder(Plot plot, ResultChartAccessor accessor)
    {
        if (accessor.XisDateTime)
            plot.Axes.DateTimeTicksBottom();

        if (accessor.XisNominal)
        {
            var ticks = accessor.GetXTicks();
            SetTicksOnAxis(plot.Axes.Bottom, ticks);
            plot.Axes.Margins(bottom: 0);
        }


        var yTicks = accessor.CalculateSeries().Select(ser =>
            new GenericTick(ser.Index, ser.Legend)).ToArray();

        SetTicksOnAxis(plot.Axes.Left, yTicks);
        plot.Axes.Margins(left: 0);

        //plot.HideGrid();
    }


    private static BarPlot CreateBars(Plot plot, Dictionary<double, double> acc,
        ResultChartAccessor.ChartSeries series, bool makeHorizontal, double barWidth)
    {
        var bars = series.X.Zip(series.Y)
            .Select(tuple =>
            {
                var x = tuple.First;
                var y = tuple.Second;
                var valBase = acc.GetValueOrDefault(x!, 0.0);
                var top = valBase + y;

                acc[x] = top;

                var bar = new Bar
                {
                    Position = x,
                    ValueBase = valBase,
                    Value = top,
                    FillColor = plot.Add.Palette.GetColor(series.Index)
                };
                if (barWidth > 0)
                    bar.Size = barWidth;

                return bar;
            }).ToArray();
        var b = plot.Add.Bars(bars);


        b.LegendText = series.Legend;
        b.Horizontal = makeHorizontal;
        return b;
    }

    private static BarPlot CreateRangeBars(Plot plot,
        ResultChartAccessor.ChartSeries series, bool makeHorizontal)
    {
        var bars = series.X.Zip(series.Y)
            .Select(tuple =>
            {
                var left = tuple.First;
                var right = tuple.Second;
                return new Bar
                {
                    Position = series.Index,
                    ValueBase = left,
                    Value = right,
                    FillColor = plot.Add.Palette.GetColor(series.Index)
                };
            }).ToArray();
        var b = plot.Add.Bars(bars);

        b.LegendText = series.Legend;
        b.Horizontal = makeHorizontal;
        return b;
    }
}

public static class TickExtension
{
    public static Tick[] ToMajorTicks(this IEnumerable<GenericTick> genericTicks)
    {
        return genericTicks.Select(kv => new Tick(kv.Value, kv.Name, true)).ToArray();
    }

    public static Tick[] ToMinorTicks(this IEnumerable<GenericTick> genericTicks)
    {
        return genericTicks.Select(kv => new Tick(kv.Value, kv.Name, false)).ToArray();
    }
}
