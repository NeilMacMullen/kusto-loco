using System.Collections.Immutable;
using System.Text;
using KustoSupport;
using NotNullStrings;

namespace KustoLoco.Rendering;

#pragma warning disable CS8618, CS8600, CS8602, CS8604
public class KustoResultRenderer
{
    public static string RenderToTable(KustoQueryResult result)
    {
        if (result.Height == 0)
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

    public static string RenderToHtml(KustoQueryResult result)
        => result.Visualization.ChartType.IsNotBlank()
            ? KustoToVegaChartType(result)
            : RenderToTable(result);


    public static string KustoToVegaChartType(KustoQueryResult result)
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
            KustoChartTypes.Scatter => RenderToChart(title, VegaMark.Point, result, NoOp,
                AllowedColumnTypes.Unrestricted),


            _ => RenderToChart(title, InferChartTypeFromResult(result), result, NoOp,
                AllowedColumnTypes.Unrestricted)
        };
    }

    private static void MakeLineChart(KustoQueryResult result, VegaChart chart)
    {
        if (result.ColumnDefinitions().Length > 2)
        {
            chart.UseCursorTooltip();
        }
    }

    private static void MakeStackedArea(KustoQueryResult result, VegaChart chart)
    {
        chart.StackAxis(VegaAxisName.Y);
    }

    private static string VisualizationKind(KustoQueryResult result)
        => result.Visualization.PropertyOr(KustoVisualizationProperties.Kind, string.Empty);

    private static void MakeBarChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == KustoVisualizationProperties.Stacked)
            b.StackAxis(VegaAxisName.X);
    }

    private static void MakeColumnChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == KustoVisualizationProperties.Stacked)
            b.StackAxis(VegaAxisName.Y);
    }

    public static void NoOp(KustoQueryResult result, VegaChart o)
    {
    }


    public static string RenderToChart(string title, VegaMark vegaType, KustoQueryResult result,
        Action<KustoQueryResult, VegaChart> jmutate, ImmutableArray<ExpectedColumnSet> expected)
    {
        if (result.Height == 0)
            return result.Error;
        var b = RenderToJObjectBuilder(vegaType, result, jmutate, expected);
        b.SetTitle(title);
        return VegaMaker.MakeHtml(title, b.Serialize());
    }


    public static void MakeTimeLineChart(KustoQueryResult result, VegaChart chart)
    {
        chart.ConvertToTimeline();
    }


    public static VegaMark InferChartTypeFromResult(KustoQueryResult result)
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
            => ex.X.Contains(c1.VegaAxisType) && ex.Y.Contains(c2.VegaAxisType);


        foreach (var e in expectedColumns)
        {
            foreach (var (x, y, z) in new[] { (0, 1, 2), (0, 2, 1), (1, 0, 2), (1, 2, 0), (2, 0, 1), (2, 1, 0) })
            {
                if (IsMatch(e, columns[x], columns[y]))
                    return [columns[x], columns[y], columns[z]];
            }
        }

        return columns;
    }

    public static VegaChart RenderToJObjectBuilder(VegaMark chartType,
        KustoQueryResult result,
        Action<KustoQueryResult, VegaChart> jmutate, ImmutableArray<ExpectedColumnSet> expectedColumns)
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
            spec.FillContainer();

        return spec;
    }

    private static VegaAxisType InferSuitableAxisType(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return InferSuitableAxisType(Nullable.GetUnderlyingType(t));
        }

        return VegaChart.InferSuitableAxisType(t);
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