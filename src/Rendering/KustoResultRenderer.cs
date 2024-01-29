using System.Collections.Immutable;
using System.Text;
using Extensions;
using JPoke;
using KustoSupport;


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

    public static string RenderToHtml(string title, KustoQueryResult result)
        => result.Visualization.ChartType.IsNotBlank()
            ? KustoToVegaChartType("title", result)
            : RenderToTable(result);


    public static string KustoToVegaChartType(string title, KustoQueryResult result)
    {
        var state = result.Visualization;
        return state.ChartType switch
        {
            "columnchart" => RenderToChart(title, VegaMark.Bar, result, MakeColumnChart, AllowedColumnTypes.ColumnChart),

            "barchart" => RenderToChart(title, VegaMark.Bar, result, MakeBarChart, AllowedColumnTypes.BarChart),
            "linechart" => RenderToChart(title, VegaMark.Line, result, NoOp, AllowedColumnTypes.LineChart),
            "piechart" => RenderToChart(title, VegaMark.Arc, result, MakePieChart, AllowedColumnTypes.ColumnChart),
            "areachart" => RenderToChart(title, VegaMark.Area, result, NoOp, AllowedColumnTypes.LineChart),
            "stackedareachart" => RenderToChart(title, VegaMark.Area, result, MakeStacked, AllowedColumnTypes.LineChart),

            "ladderchart" => RenderToChart(title, VegaMark.Bar, result, MakeTimeLineChart, AllowedColumnTypes.LadderChart),
            "scatterchart" => RenderToChart(title, VegaMark.Point, result, MakeScatter, AllowedColumnTypes.Unrestricted),


            _ => RenderToChart(title, InferChartTypeFromResult(result), result, NoOp, AllowedColumnTypes.Unrestricted)
        };
    }

    private static string VisualizationKind(KustoQueryResult result)
    {
        if (result.Visualization.Items.TryGetValue("kind", out var v) && v is string k)
            return k;
        return "default";
    }

    private static void MakeBarChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == "stacked")
        {
            b._builder.Set("encoding.x.aggregate", "sum");
        }
    }

    private static void MakeColumnChart(KustoQueryResult result, VegaChart b)
    {
        if (VisualizationKind(result) == "stacked")
        {
            b._builder.Set("encoding.y.aggregate", "sum");
        }
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
        return VegaMaker.MakeHtml(title, b.Serialize());
    }


    public static void MakePieChart(KustoQueryResult result, VegaChart b)
    {
    }


    public static void MakeTimeLineChart(KustoQueryResult result, VegaChart chart)
    {
        var b = chart._builder;
        b.Set("mark.type", VegaMark.Bar);
        b.Move("encoding.y", "encoding.x2");

        b.Set("encoding.y.type", VegaAxisType.Ordinal);
        b.Copy("encoding.color.title", "encoding.y.title");
        b.Copy("encoding.color.field", "encoding.y.field");
        b.Set("config.legend.disable", true);
    }

    public static void MakeScatter(KustoQueryResult result, VegaChart b)
    {
    }


    public static void MakeGridChart(VegaChart chart)
    {
        var b = chart._builder;
        b.Set("config.axis.grid", "true");
        b.Set("config.axis.tickband", "extent");
    }

    public static void MakeStacked(KustoQueryResult result, VegaChart chart)
    {
        var b = chart._builder;
        b.Set("encoding.y.aggregate", "sum");
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
        var allColumns = headers.Select(h => new ColumnDescription(h.Name, h.Name,
                InferSuitableAxisType(h.UnderlyingType)))
            .Concat(Enumerable.Range(0, 3).Select(i => new ColumnDescription(string.Empty, string.Empty, VegaAxisType.Nominal)))
            .Take(3)
            .ToArray();

        return allColumns;
    }

    //try to reorder columns in a way that makes most sensor for the desired chart type
    public static ColumnDescription[] TryMatch(ColumnDescription[] columns, ImmutableArray<ExpectedColumnSet> expectedColumns)
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

    public static JObjectBuilder RenderToJObjectBuilder(VegaMark chartType,
        KustoQueryResult result,
        Action<KustoQueryResult, VegaChart> jmutate, ImmutableArray<ExpectedColumnSet> expectedColumns)
    {
        var columns = GetColumns(result);

        columns = TryMatch(columns, expectedColumns);

        var spec = VegaChart.CreateVegaChart(
            chartType,
            columns[0],
            columns[1],
            columns[2]
        );


        var b = spec._builder;
        var rows = result.AsOrderedDictionarySet();

        b.Set("data.values", rows);
        jmutate(result, spec);

        b.Set("width", "container");
        b.Set("height", "container");
        return b;
    }

    private static VegaAxisType InferSuitableAxisType(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return InferSuitableAxisType(Nullable.GetUnderlyingType(t));
        }

        var numericTypes = new[]
        {
            typeof(int), typeof(long), typeof(double), typeof(float),
        };
        if (numericTypes.Contains(t))
            return VegaAxisType.Quantitative;

        return t == typeof(DateTime)
            ? VegaAxisType.Temporal
            : VegaAxisType.Nominal;
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
    public static ImmutableArray<ExpectedColumnSet> Unrestricted = ImmutableArray<ExpectedColumnSet>.Empty;

    public static ImmutableArray<ExpectedColumnSet> LineChart =
        [new ExpectedColumnSet([VegaAxisType.Quantitative,VegaAxisType.Temporal], [VegaAxisType.Quantitative])];

    public static ImmutableArray<ExpectedColumnSet> BarChart =
        [new ExpectedColumnSet([VegaAxisType.Quantitative,VegaAxisType.Temporal], [VegaAxisType.Nominal,VegaAxisType.Ordinal])];

    public static ImmutableArray<ExpectedColumnSet> ColumnChart =
    [
        new ExpectedColumnSet([VegaAxisType.Temporal,VegaAxisType.Ordinal], [VegaAxisType.Quantitative,VegaAxisType.Temporal])
    ];

    public static ImmutableArray<ExpectedColumnSet> LadderChart =
        [new ExpectedColumnSet([VegaAxisType.Temporal],[VegaAxisType.Temporal])];
}