using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using NotNullStrings;

namespace KustoLoco.Rendering;

public class KustoResultRenderer
{
    private readonly KustoSettingsProvider _settings;

    public KustoResultRenderer(KustoSettingsProvider settings)
    {
        _settings = settings;
    }

    public string RenderToTable(KustoQueryResult result)
    {
        if (result.RowCount == 0)
            return result.Error;
        var headers = result.ColumnNames();
        var defaultMax = 100;
        var settingName = "html.maxtablerows";
        var maxRows = _settings.GetIntOr(settingName, defaultMax);
     
        var sb = new StringBuilder();


        if (result.RowCount > maxRows)
            sb.AppendLine($@"<p><font color=""red"">Only first {maxRows} of {result.RowCount} displayed.  " +
                                                  $"Set {settingName} to see more.</p>");
        sb.AppendLine(
            @"<style>
table {
  border-collapse: collapse;
  width: 100%;
}

th
{
  text-align: left;
  padding: 8px;
   background-color: #00D000;
}

td {
  text-align: left;
  padding: 8px;
  
}

tr:nth-child(even) {
  background-color: #D6EEEE;
}

tr:nth-child(odd) {
  background-color: #D0D0D0;
}
</style>
");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr>");
        foreach (var h in headers)
            sb.AppendLine($"<th>{h}</th>");
        foreach (var r in result.EnumerateRows().Take(maxRows))
        {
            sb.AppendLine("<tr>");
            var line = string.Concat(r.Select(item => $"<td>{item}</td>"));
            sb.AppendLine(line);

            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        return sb.ToString();
    }

    public string RenderToHtml(KustoQueryResult result)
    {
        return result.Error.IsNotBlank()
            ? RenderStringAsHtml(result.Error)
            : result.RowCount == 0
                ? RenderStringAsHtml("No results")
                : result.Visualization.ChartType.IsNotBlank()
                    ? KustoToVegaChartType(result)
                    : RenderToTable(result);
    }

    public string RenderStringAsHtml(string x)
        => "<HTML><BODY><font color=\"green\">" + x + "</font></BODY></HTML>";

    public void RenderToComposer(KustoQueryResult result, VegaComposer composer)
    {
        if (result.Error.IsNotBlank())
            composer.AddRawHtml("<p>result.Error</p>");
        else if (result.Visualization.ChartType.IsNotBlank())
            KustoToVegaChartType(result, composer);
        else
            composer.AddRawHtml(RenderToTable(result));
    }

    public string KustoToVegaChartType(KustoQueryResult result)
    {
        var theme = _settings.GetOr("vega.theme", "dark");
        var composer = new VegaComposer("composed", theme);
        KustoToVegaChartType(result, composer);
        return composer.Render();
    }

    public void KustoToVegaChartType(KustoQueryResult result, VegaComposer composer)
    {
        var state = result.Visualization;
        switch (state.ChartType)
        {
            case KustoChartTypes.Column:
                RenderToChart(VegaMark.Bar, result, MakeColumnChart, AllowedColumnTypes.ColumnChart, composer);
                break;
            case KustoChartTypes.Bar:
                RenderToChart(VegaMark.Bar, result, MakeBarChart,
                    AllowedColumnTypes.BarChart, composer);
                break;
            case KustoChartTypes.Line:
                RenderToChart(VegaMark.Line, result, MakeLineChart,
                    AllowedColumnTypes.LineChart, composer);
                break;
            case KustoChartTypes.Pie:
                RenderToChart(VegaMark.Arc, result, NoOp,
                    AllowedColumnTypes.ColumnChart, composer);
                break;
            case KustoChartTypes.Area:
                RenderToChart(VegaMark.Area, result, NoOp,
                    AllowedColumnTypes.LineChart, composer);
                break;
            case KustoChartTypes.StackedArea:
                RenderToChart(VegaMark.Area, result, MakeStackedArea,
                    AllowedColumnTypes.LineChart, composer);
                break;
            case KustoChartTypes.Ladder:
                RenderToChart(VegaMark.Bar, result, MakeTimeLineChart,
                    AllowedColumnTypes.LadderChart, composer);
                break;
            case KustoChartTypes.Scatter:
                RenderToChart(VegaMark.Point, result, MakeScatterChart,
                    AllowedColumnTypes.Unrestricted, composer);
                break;
            default:
                RenderToChart(InferChartTypeFromResult(result), result, NoOp,
                    AllowedColumnTypes.Unrestricted, composer);
                break;
        }
    }

    private static void MakeLineChart(KustoQueryResult result, VegaChart chart)
    {
        if (result.ColumnDefinitions().Length > 2) chart.UseCursorTooltip();
    }

    private static void MakeStackedArea(KustoQueryResult result, VegaChart chart)
        => chart.StackAxis(VegaAxisName.Y);

    private static string VisualizationKind(KustoQueryResult result) => result.Visualization.PropertyOr(KustoVisualizationProperties.Kind, string.Empty);

    private static void MakeBarChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == KustoVisualizationProperties.Stacked)
            b.StackAxis(VegaAxisName.X);
        //check for case where we are trying to draw "true" barchart oriented horizontally
        var ytype =b._builder.Get("encoding.y.type","");
        if (ytype != "nominal")
            b._builder.Set($"encoding.x.type", "ordinal");
    }

    private static void MakeColumnChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == KustoVisualizationProperties.Stacked)
            b.StackAxis(VegaAxisName.Y);
    }

    private static void NoOp(KustoQueryResult result, VegaChart o)
    {
    }


    private void RenderToChart(VegaMark vegaType, KustoQueryResult result,
        Action<KustoQueryResult, VegaChart> jmutate, ImmutableArray<ExpectedColumnSet> expected, VegaComposer composer)
    {
        if (result.RowCount == 0)
        {
            composer.AddRawHtml("<p>No results</p>");
            return;
        }

        var title = result.Visualization.PropertyOr("title", DateTime.Now.ToShortTimeString());
        var b = RenderToJObjectBuilder(vegaType, result, jmutate, expected);
        b.SetTitle(title);
        composer.AddChart(b);
    }


    public static void RenderChartInBrowser(KustoQueryResult result)
    {
        new KustoResultRenderer(new KustoSettingsProvider()).RenderInBrowser(result);
    }

    /// <summary>
    ///     If the query result has a chart type, render it in the browser by creating a temporary html file
    /// </summary>
    public void RenderInBrowser(KustoQueryResult result)
    {
        if (result.Visualization.ChartType.IsBlank())
            return;

        var fileName = Path.ChangeExtension(Path.GetTempFileName(), "html");
        var text = RenderToHtml(result);
        File.WriteAllText(fileName, text);
        Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
    }

    public void MakeTimeLineChart(KustoQueryResult result, VegaChart chart)
    {
        chart.ConvertToTimeline();
    }


    public void MakeScatterChart(KustoQueryResult result, VegaChart chart)
    {
        foreach (var c in "size shape angle".Tokenize())
        {
            var markSize = _settings.GetOr($"vega.point.{c}", string.Empty);
            if (markSize.IsNotBlank())
                chart._builder.Set($"mark.{c}", markSize);
        }
    }


    private static VegaMark InferChartTypeFromResult(KustoQueryResult result)
    {
        var headers = result.ColumnDefinitions();
        var types = headers.Select(h => h.UnderlyingType).ToArray();

        return InferChartTypeFromAxisTypes(types.Select(InferSuitableAxisType).ToArray());
    }

    public static ColumnDescription[] GetColumns(KustoQueryResult result)
    {
        var headers = result.ColumnDefinitions();
        var needed = Math.Max(3 - headers.Length, 0);
        var allColumns = headers.Select(h => new ColumnDescription(h.Name, h.Name,
                InferSuitableAxisType(h.UnderlyingType)))
            .Concat(Enumerable.Range(0, needed)
                .Select(i => new ColumnDescription(string.Empty, string.Empty,
                    VegaAxisType.Nominal)))
            .ToArray();

        return allColumns;
    }

    //try to reorder columns in a way that makes most sensor for the desired chart type
    public static ColumnDescription[] TryMatch(ColumnDescription[] columns,
        ImmutableArray<ExpectedColumnSet> expectedColumns)
    {
        foreach (var e in expectedColumns)
        foreach (var (x, y, z) in new[] { (0, 1, 2), (0, 2, 1), (1, 0, 2), (1, 2, 0), (2, 0, 1), (2, 1, 0) })
            if (IsMatch(e, columns[x], columns[y]))
                return [columns[x], columns[y], columns[z]];

        return columns;

        bool IsMatch(ExpectedColumnSet ex, ColumnDescription c1, ColumnDescription c2)
            => ex.X.Contains(c1.VegaAxisType) && ex.Y.Contains(c2.VegaAxisType);
    }

    public static VegaChart RenderToJObjectBuilder(VegaMark chartType,
        KustoQueryResult result,
        Action<KustoQueryResult, VegaChart> jmutate, ImmutableArray<ExpectedColumnSet> expectedColumns
    )
    {
        var columns = GetColumns(result);

        // columns = TryMatch(columns, expectedColumns);

        var spec = VegaChart.CreateVegaChart(
            chartType,
            columns[0],
            columns[1],
            columns[2]
        );


        var rows = result.AsOrderedDictionarySet();
        spec.InjectData(rows);

        jmutate(result, spec);


        if (columns.Length >= 4)
        {
            spec.AddFacet(columns[3]);
            spec.SetSize(1800, 100);
        }
        else
        {
            spec.FillContainer();
        }
        FixBinning(columns[0],"x");
        FixBinning(columns[1],"y");

        return spec;

        //set appropriate x axis scale if this is a time series
        void FixBinning(ColumnDescription column, string axis)
        {
            if (column.VegaAxisType != VegaAxisType.Temporal) return;
            var q = column.QualifiedColumnName;
            var data = rows.Select(r => r[q]).Cast<DateTime>().ToArray();
            var u = "";
            if (data.DistinctBy(d => d.Year).Count() > 1)
                u += "year";
            if (data.DistinctBy(d => d.Month).Count() > 1)
                u += "month";
            if (data.DistinctBy(d => d.Day).Count() > 1)
                u += "date";
            if (data.DistinctBy(d => d.Hour).Count() > 1)
                u += "hours";
            if (data.DistinctBy(d => d.Hour).Count() > 1)
                u += "minutes";
            if (u.IsNotBlank())
            {
                spec._builder.Set($"encoding.{axis}.timeUnit", $"binned{u}");
                spec._builder.Set($"encoding.{axis}.type", "ordinal");
            }
        }
    }

    private static VegaAxisType InferSuitableAxisType(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? InferSuitableAxisType(Nullable.GetUnderlyingType(t)!)
            : VegaChart.InferSuitableAxisType(t);
    }

    private static VegaMark InferChartTypeFromAxisTypes(VegaAxisType[] axisTypes)
    {
        var x = axisTypes[0];
        var y = axisTypes.Skip(1).ToArray();

        return x == VegaAxisType.Ordinal
            ? VegaMark.Bar
            : VegaMark.Line;
    }
}

public readonly record struct ExpectedColumnSet(VegaAxisType[] X, VegaAxisType[] Y);

public static class AllowedColumnTypes
{
    public static ImmutableArray<ExpectedColumnSet> Unrestricted = [];

    public static ImmutableArray<ExpectedColumnSet> LineChart =
        [new([VegaAxisType.Quantitative, VegaAxisType.Temporal], [VegaAxisType.Quantitative])];

    public static ImmutableArray<ExpectedColumnSet> BarChart =
    [
        new([VegaAxisType.Quantitative, VegaAxisType.Temporal],
            [VegaAxisType.Nominal, VegaAxisType.Ordinal])
    ];

    public static ImmutableArray<ExpectedColumnSet> ColumnChart =
    [
        new([VegaAxisType.Temporal, VegaAxisType.Ordinal],
            [VegaAxisType.Quantitative, VegaAxisType.Temporal])
    ];

    public static ImmutableArray<ExpectedColumnSet> LadderChart =
        [new([VegaAxisType.Temporal], [VegaAxisType.Temporal])];
}

public static class KustoChartTypes
{
    public const string Column = "columnchart";

    public const string Bar = "barchart";
    public const string Line = "linechart";
    public const string Pie = "piechart";
    public const string Area = "areachart";
    public const string StackedArea = "stackedareachart";
    public const string Ladder = "ladderchart";
    public const string Scatter = "scatterchart";
}

public static class KustoVisualizationProperties
{
    public const string Stacked = "stacked";
    public const string Kind = "kind";
}
