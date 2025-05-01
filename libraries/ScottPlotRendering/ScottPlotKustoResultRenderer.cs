using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.SixelSupport;
using NotNullStrings;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;

namespace KustoLoco.Rendering.ScottPlot;

public static class ScottPlotKustoResultRenderer
{
    private const string StandardAxisPreferences = "tno|nno|ono|tn|nn|on|to";


    private static void SetColorFromSetting(KustoSettingsProvider settings, string settingName, Action<Color> setter,
        string defaultColour)
    {
        var s = settings.GetOr(settingName, defaultColour);
        try
        {
            var col = Color.FromHex(s);
            setter(col);
        }
        catch
        {
            setter(Color.FromHex(defaultColour));
        }
    }

    public static void SetInitialUiPreferences(KustoQueryResult result, Plot plot, KustoSettingsProvider settings)
    {
        var paletteName = settings.GetOr("scottplot.palette", "");

        var ipallette = Palette.GetPalettes()
            .FirstOrDefault(f => f.Name.Equals(paletteName, StringComparison.InvariantCultureIgnoreCase));
        if (ipallette == null)
        {
            //allow palettes to be defined as lists of hex colors
            var custom = paletteName.Tokenize(" ,;");
            ipallette = custom.Length > 1
                ? Palette.FromColors(custom)
                : new Penumbra();
        }

        plot.Add.Palette = ipallette;
        SetColorFromSetting(settings, "scottplot.figurebackground.color", c => plot.FigureBackground.Color = c,
            "#181818");
        SetColorFromSetting(settings, "scottplot.databackground.color", c => plot.DataBackground.Color = c, "#1f1f1f");
        SetColorFromSetting(settings, "scottplot.axes.color", c => plot.Axes.Color(c), "#ffffff");
        SetColorFromSetting(settings, "scottplot.majorlinecolor", c => plot.Grid.MajorLineColor = c, "#404040");

        // change legend colors
        SetColorFromSetting(settings, "scottplot.legend.backgroundcolor", c => plot.Legend.BackgroundColor = c,
            "#404040");

        SetColorFromSetting(settings, "scottplot.legend.fontcolor", c => plot.Legend.FontColor = c, "#d7d7d7");
        SetColorFromSetting(settings, "scottplot.legend.outlinecolor", c => plot.Legend.OutlineColor = c, "#d7d7d7");

        var legendFontSize = settings.GetIntOr("scottplot.legend.fontsize", 12);
        plot.Legend.FontSize = legendFontSize;
        plot.Title(result.Visualization.PropertyOr("title", string.Empty));

        if (result.Visualization.PropertyOr("legend", "") == "hidden")
        {
            plot.Legend.IsVisible = false;
        }
        else
        {
            var tokens = settings.GetOr("scottplot.legend.placement", "right").Tokenize();
            var edge = Edge.Right;
            if (tokens.Any(t => Enum.TryParse(t, true, out edge)))
            {
                plot.ShowLegend(edge);
            }
            else
            {
                //there seems to be some deadlock if we try to set both alignment/orientation
                //at the same time as declaring an edge
                var alignment = Alignment.UpperRight;
                if (tokens.Any(t => Enum.TryParse(t, true, out alignment)))
                    plot.Legend.Alignment = alignment;
                var orientation = Orientation.Vertical;
                if (tokens.Any(t => Enum.TryParse(t, true, out orientation)))
                    plot.Legend.Orientation = orientation;
            }
        }
    }

    /// <summary>
    ///     Renders various types of charts based on the provided query results and settings.
    /// </summary>
    public static void RenderToPlot(Plot plot, KustoQueryResult result, KustoSettingsProvider settings)
    {
        var accessor = new ResultChartAccessor(result);
        plot.Clear();
        SetInitialUiPreferences(result, plot, settings);
        if (accessor.Kind() == ResultChartAccessor.ChartKind.Pie && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, "on|ot", 0, 1, 0);

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
            // hide unnecessary plot components
            plot.Axes.Frameless();
            plot.HideGrid();
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Line && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPreferences, 0, 1, 2);
            foreach (var ser in accessor.CalculateSeries())
            {
                //for lines, it's important that order by X otherwise
                //we'll get a real spiderweb
                var ordered = ser.OrderByX();
                var line = plot.Add.Scatter(ordered.X, ordered.Y);
                line.LegendText = ordered.Legend;
                line.LineWidth = (float)settings.GetDoubleOr("scottplot.line.linewidth",
                    line.LineWidth);
                line.MarkerSize = (float)settings.GetDoubleOr("scottplot.line.markersize",
                    line.MarkerSize);
            }

            FixupAxisTicks(plot, accessor, false);
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Scatter && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPreferences, 0, 1, 2);
            foreach (var ser in accessor.CalculateSeries())
            {
                var line = plot.Add.ScatterPoints(ser.X, ser.Y);
                var markerSize = settings.GetDoubleOr("scottplot.scatter.markersize", line.MarkerSize);
                line.MarkerSize = (float)markerSize;
                line.LegendText = ser.Legend;
            }

            FixupAxisTicks(plot, accessor, false);
        }


        if (accessor.Kind() == ResultChartAccessor.ChartKind.Column && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPreferences, 0, 1, 2);
            var acc = accessor.CreateAccumulatorForStacking();
            var barWidth = accessor.GetSuggestedBarWidth();
            var bars = accessor.CalculateSeries()
                .Select(ser => CreateBars(plot, acc, ser, false, barWidth))
                .ToArray();

            FixupAxisTicks(plot, accessor, false);
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Bar && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, StandardAxisPreferences, 0, 1, 2);
            var acc = accessor.CreateAccumulatorForStacking();
            var barWidth = accessor.GetSuggestedBarWidth();
            var bars = accessor.CalculateSeries()
                .Select(ser => CreateBars(plot, acc, ser, true, barWidth))
                .ToArray();

            FixupAxisTicks(plot, accessor, true);
        }

        if (accessor.Kind() == ResultChartAccessor.ChartKind.Ladder && result.ColumnCount >= 2)
        {
            StandardAxisAssignment(accessor, "tto|nno", 0, 1, 2);

            var bars = accessor.CalculateSeries()
                .Select(ser => CreateRangeBars(plot, ser, true))
                .ToArray();
            FixupAxisForLadder(plot, accessor);
        }

        FinishUIPreferences(plot, settings);
    }

    private static void FinishUIPreferences(Plot plot, KustoSettingsProvider settings)
    {
        plot.Axes.Bottom.Label.FontSize = settings.GetIntOr("scottplot.axes.bottom.label.fontsize", 12);
        plot.Axes.Left.Label.FontSize = settings.GetIntOr("scottplot.axes.left.label.fontsize", 12);
        plot.Axes.Bottom.Label.Rotation = (float)settings.GetDoubleOr("scottplot.axes.bottom.label.rotation", 0.0);
        plot.Axes.Left.Label.Rotation = (float)settings.GetDoubleOr("scottplot.axes.left.label.rotation", -90.0);

        plot.Axes.Bottom.TickLabelStyle.Rotation =
            (float)settings.GetDoubleOr("scottplot.axes.bottom.ticklabelstyle.rotation", 0.0);
        plot.Axes.Left.TickLabelStyle.Rotation =
            (float)settings.GetDoubleOr("scottplot.axes.left.ticklabelstyle.rotation", 0.0);
        plot.Axes.Bottom.TickLabelStyle.FontSize =
            settings.GetIntOr("scottplot.axes.bottom.ticklabelstyle.fontsize", 12);
        plot.Axes.Left.TickLabelStyle.FontSize = settings.GetIntOr("scottplot.axes.left.ticklabelstyle.fontsize", 12);
    }

    private static void StandardAxisAssignment(ResultChartAccessor accessor,
        string preferences, int x, int y, int s)
    {
        var cols = accessor.TryOrdering(preferences);
        accessor.AssignXColumn(cols[x].Index);
        accessor.AssignValueColumn(cols[y].Index);
        if (s < cols.Length)
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
                MakeXAxisDateTime(plot);
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
                MakeXAxisDateTime(plot);

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
    private static void MakeYAxisDateTime(Plot plot) => plot.Axes.Left.TickGenerator = new FixedDateTimeAutomatic();
    private static void MakeXAxisDateTime(Plot plot) => plot.Axes.Bottom.TickGenerator = new FixedDateTimeAutomatic();


    private static void FixupAxisForLadder(Plot plot, ResultChartAccessor accessor)
    {
        if (accessor.XisDateTime)
            MakeXAxisDateTime(plot);

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

    /// <summary>
    ///     Renders a KustoQueryResult to a Sixel
    /// </summary>
    public static string RenderToSixel(
        KustoQueryResult result,
        KustoSettingsProvider settings,
        int widthInPixels,
        int heightInPixels)
    {
        using var plot = new Plot();
        RenderToPlot(plot, result, settings);
        var bytes = plot.GetImageBytes(widthInPixels, heightInPixels);
        var src = ArgbPixelSource.FromScottPlotBmp(bytes);

        return SixelMaker.FrameToSixelString(src);
    }

    /// <summary>
    ///     Attempts to generate a sixel that fits the current screen
    /// </summary>
    public static string RenderToSixel(
        KustoQueryResult result,
        KustoSettingsProvider settings)
    {
        var (width, height) = TerminalHelper.GetScreenDimension();
        return RenderToSixel(result, settings, width, height);
    }

    /// <summary>
    ///     Attempts to generate a sixel that fits the current screen
    /// </summary>
    public static string RenderToSixelWithPad(
        KustoQueryResult result,
        KustoSettingsProvider settings, int linesAtEnd)
    {
        var (width, height) = TerminalHelper.GetScreenDimension(linesAtEnd);
        return RenderToSixel(result, settings, width, height);
    }
}
