using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Microsoft.VisualBasic.CompilerServices;
using NotNullStrings;

namespace KustoLoco.Rendering;

public class KustoResultRenderer
{
    private readonly KustoSettingsProvider _settings;

    public KustoResultRenderer(KustoSettingsProvider settings)
    {
        _settings = settings;
    }

    public static string RenderToTable(KustoQueryResult result)
    {
        if (result.RowCount == 0)
            return result.Error;
        var headers = result.ColumnNames();


        var sb = new StringBuilder();
        sb.AppendLine("<table>");
        sb.AppendLine("<tr>");
        foreach (var h in headers)
            sb.AppendLine($"<th>{h}</th>");
        foreach (var r in result.EnumerateRows())
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
            ? result.Error
            : result.Visualization.ChartType.IsNotBlank()
                ? KustoToVegaChartType(result)
                : RenderToTable(result);
    }


    public string KustoToVegaChartType(KustoQueryResult result)
    {
        var title = result.Visualization.PropertyOr("title", DateTime.Now.ToShortTimeString());
        var state = result.Visualization;
        return state.ChartType switch
        {
            KustoChartTypes.Column => RenderToChart(title, VegaMark.Bar, result, MakeColumnChart,
                AllowedColumnTypes.ColumnChart),

            KustoChartTypes.Bar => RenderToChart(title, VegaMark.Bar, result, MakeBarChart,
                AllowedColumnTypes.BarChart),
            KustoChartTypes.Line => RenderToChart(title, VegaMark.Line, result, MakeLineChart,
                AllowedColumnTypes.LineChart),
            KustoChartTypes.Pie => RenderToChart(title, VegaMark.Arc, result, NoOp,
                AllowedColumnTypes.ColumnChart),
            KustoChartTypes.Area => RenderToChart(title, VegaMark.Area, result, NoOp,
                AllowedColumnTypes.LineChart),
            KustoChartTypes.StackedArea => RenderToChart(title, VegaMark.Area, result, MakeStackedArea,
                AllowedColumnTypes.LineChart),

            KustoChartTypes.Ladder => RenderToChart(title, VegaMark.Bar, result, MakeTimeLineChart,
                AllowedColumnTypes.LadderChart),
            KustoChartTypes.Scatter => RenderToChart(title, VegaMark.Point, result, MakeScatterChart,
                AllowedColumnTypes.Unrestricted),


            _ => RenderToChart(title, InferChartTypeFromResult(result), result, NoOp,
                AllowedColumnTypes.Unrestricted)
        };
    }

    private void MakeLineChart(KustoQueryResult result, VegaChart chart)
    {
        if (result.ColumnDefinitions().Length > 2) chart.UseCursorTooltip();
    }

    private void MakeStackedArea(KustoQueryResult result, VegaChart chart)
    {
        chart.StackAxis(VegaAxisName.Y);
    }

    private string VisualizationKind(KustoQueryResult result)
    {
        return result.Visualization.PropertyOr(KustoVisualizationProperties.Kind, string.Empty);
    }

    private void MakeBarChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == KustoVisualizationProperties.Stacked)
            b.StackAxis(VegaAxisName.X);
    }

    private void MakeColumnChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == KustoVisualizationProperties.Stacked)
            b.StackAxis(VegaAxisName.Y);
    }

    public void NoOp(KustoQueryResult result, VegaChart o)
    {
    }


    public string RenderToChart(string title, VegaMark vegaType, KustoQueryResult result,
        Action<KustoQueryResult, VegaChart> jmutate, ImmutableArray<ExpectedColumnSet> expected)
    {
        if (result.RowCount == 0)
            return result.Error;
        var b = RenderToJObjectBuilder(vegaType, result, jmutate, expected);
        b.SetTitle(title);
        var theme = _settings.GetOr("vega.theme", "dark");
        return VegaMaker.MakeHtml(title, b.Serialize(),theme);
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

    public  void MakeTimeLineChart(KustoQueryResult result, VegaChart chart)
    {
        chart.ConvertToTimeline();
    }


    public  void MakeScatterChart(KustoQueryResult result, VegaChart chart)
    {
        foreach (var c in "size shape angle".Tokenize())
        {
            var markSize = _settings.GetOr($"vega.point.{c}", string.Empty);
            if (markSize.IsNotBlank())
                chart._builder.Set($"mark.{c}", markSize);
        }
    }


    public  VegaMark InferChartTypeFromResult(KustoQueryResult result)
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
        bool IsMatch(ExpectedColumnSet ex, ColumnDescription c1, ColumnDescription c2)
        {
            return ex.X.Contains(c1.VegaAxisType) && ex.Y.Contains(c2.VegaAxisType);
        }


        foreach (var e in expectedColumns)
        foreach (var (x, y, z) in new[] { (0, 1, 2), (0, 2, 1), (1, 0, 2), (1, 2, 0), (2, 0, 1), (2, 1, 0) })
            if (IsMatch(e, columns[x], columns[y]))
                return [columns[x], columns[y], columns[z]];

        return columns;
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

        return spec;
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

        if (x == VegaAxisType.Ordinal)
            return VegaMark.Bar;
        return VegaMark.Line;
    }
}

public readonly record struct ExpectedColumnSet(VegaAxisType[] X, VegaAxisType[] Y);

public static class AllowedColumnTypes
{
    public static ImmutableArray<ExpectedColumnSet> Unrestricted = [];

    public static ImmutableArray<ExpectedColumnSet> LineChart =
        [new ExpectedColumnSet([VegaAxisType.Quantitative, VegaAxisType.Temporal], [VegaAxisType.Quantitative])];

    public static ImmutableArray<ExpectedColumnSet> BarChart =
    [
        new ExpectedColumnSet([VegaAxisType.Quantitative, VegaAxisType.Temporal],
            [VegaAxisType.Nominal, VegaAxisType.Ordinal])
    ];

    public static ImmutableArray<ExpectedColumnSet> ColumnChart =
    [
        new ExpectedColumnSet([VegaAxisType.Temporal, VegaAxisType.Ordinal],
            [VegaAxisType.Quantitative, VegaAxisType.Temporal])
    ];

    public static ImmutableArray<ExpectedColumnSet> LadderChart =
        [new ExpectedColumnSet([VegaAxisType.Temporal], [VegaAxisType.Temporal])];
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
