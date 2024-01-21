using System.Collections.Immutable;
using System.Text;
using Extensions;
using JPoke;
using KustoSupport;
using static VegaGenerator;

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
            "columnchart" => RenderToChart(title, BarChart, result, MakeColumnChart, AllowedColumnTypes.ColumnChart),

            "barchart" => RenderToChart(title, BarChart, result, MakeBarChart, AllowedColumnTypes.BarChart),
            "linechart" => RenderToChart(title, LineChart, result, NoOp, AllowedColumnTypes.LineChart),
            "piechart" => RenderToChart(title, PieChart, result, MakePieChart, AllowedColumnTypes.ColumnChart),
            "areachart" => RenderToChart(title, AreaChart, result, NoOp, AllowedColumnTypes.LineChart),
            "stackedareachart" => RenderToChart(title, AreaChart, result, MakeStacked, AllowedColumnTypes.LineChart),

            "ladderchart" => RenderToChart(title, BarChart, result, MakeTimeLineChart, AllowedColumnTypes.LadderChart),
            "scatterchart" => RenderToChart(title, "point", result, MakeScatter, AllowedColumnTypes.Unrestricted),


            _ => RenderToChart(title, InferChartTypeFromResult(result), result, NoOp, AllowedColumnTypes.Unrestricted)
        };
    }

    private static string VisualizationKind(KustoQueryResult result)
    {
        if (result.Visualization.Items.TryGetValue("kind", out var v) && v is string k)
            return k;
        return "default";
    }

    private static void MakeBarChart(KustoQueryResult result, JObjectBuilder b)
    {
        if (VisualizationKind(result) == "stacked")
        {
            b.Set("encoding.x.aggregate", "sum");
        }
    }

    private static void MakeColumnChart(KustoQueryResult result, JObjectBuilder b)
    {
        if (VisualizationKind(result) == "stacked")
        {
            b.Set("encoding.y.aggregate", "sum");
        }
    }

    public static void NoOp(KustoQueryResult result, JObjectBuilder o)
    {
    }


    public static string RenderToChart(string title, string vegaType, KustoQueryResult result,
        Action<KustoQueryResult, JObjectBuilder> jmutate, ImmutableArray<ExpectedColumnSet> expected)
    {
        if (result.Height == 0)
            return result.Error;
        var b = RenderToJObjectBuilder(vegaType, result, jmutate, expected);
        return VegaMaker.MakeHtml(title, b.Serialize());
    }


    public static void MakePieChart(KustoQueryResult result, JObjectBuilder b)
    {
        b.Set("mark.type", PieChart);
        b.Move("encoding.y", "encoding.theta");
        b.Set("encoding.color.type", AxisTypeNominal);
        b.Copy("encoding.x.field", "encoding.color.field");
        b.Remove("encoding.y");
        b.Remove("encoding.x");
    }


    public static void MakeTimeLineChart(KustoQueryResult result, JObjectBuilder b)
    {
        b.Set("mark.type", BarChart);
        b.Move("encoding.y", "encoding.x2");

        b.Set("encoding.y.type", AxisTypeOrdinal);
        b.Copy("encoding.color.title", "encoding.y.title");
        b.Copy("encoding.color.field", "encoding.y.field");
        b.Set("config.legend.disable", true);
    }

    public static void MakeScatter(KustoQueryResult result, JObjectBuilder b)
    {
    }


    public static void MakeGridChart(JObjectBuilder b)
    {
        b.Set("config.axis.grid", "true");
        b.Set("config.axis.tickband", "extent");
    }

    public static void MakeStacked(KustoQueryResult result, JObjectBuilder b)
    {
        b.Set("encoding.y.aggregate", "sum");
    }

    public static string InferChartTypeFromResult(KustoQueryResult result)
    {
        var headers = result.ColumnDefinitions();
        var types = headers.Select(h => h.UnderlyingType).ToArray();

        return InferChartTypeFromAxisTypes(types.Select(InferSuitableAxisType).ToArray());
    }

    public static ColumnAndName[] GetColumns(KustoQueryResult result)
    {
        var headers = result.ColumnDefinitions();
        var allColumns = headers.Select(h => new ColumnAndName(h.Name, h.Name,
                InferSuitableAxisType(h.UnderlyingType)))
            .Concat(Enumerable.Range(0, 3).Select(i => new ColumnAndName(string.Empty, string.Empty, string.Empty)))
            .Take(3)
            .ToArray();

        return allColumns;
    }

    //try to reorder columns in a way that makes most sensor for the desired chart type
    public static ColumnAndName[] TryMatch(ColumnAndName[] columns, ImmutableArray<ExpectedColumnSet> expectedColumns)
    {
        bool IsMatch(ExpectedColumnSet ex, ColumnAndName c1, ColumnAndName c2)
            => ex.X.Contains(c1.VegaSeriesType) && ex.Y.Contains(c2.VegaSeriesType);


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

    public static JObjectBuilder RenderToJObjectBuilder(string chartType,
        KustoQueryResult result,
        Action<KustoQueryResult, JObjectBuilder> jmutate, ImmutableArray<ExpectedColumnSet> expectedColumns)
    {
        var columns = GetColumns(result);

        columns = TryMatch(columns, expectedColumns);

        var spec = Spec(
            chartType,
            columns[0],
            columns[1],
            columns[2]
        );


        var b = JObjectBuilder.FromObject(spec);
        var rows = result.AsOrderedDictionarySet();

        b.Set("data.values", rows);
        jmutate(result, b);

        b.Set("width", "container");
        b.Set("height", "container");
        return b;
    }

    private static string InferSuitableAxisType(Type t)
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
            return AxisTypeQuantity;

        return t == typeof(DateTime)
            ? AxisTypeTime
            : AxisTypeOrdinal;
    }

    private static string InferChartTypeFromAxisTypes(string[] axisTypes)
    {
        var x = axisTypes[0];
        var y = axisTypes.Skip(1).ToArray();

        if (x == AxisTypeOrdinal)
            return BarChart;
        return LineChart;
    }
}

public readonly record struct ExpectedColumnSet(string X, string Y);

public static class AllowedColumnTypes
{
    public static ImmutableArray<ExpectedColumnSet> Unrestricted = ImmutableArray<ExpectedColumnSet>.Empty;

    public static ImmutableArray<ExpectedColumnSet> LineChart =
        [new ExpectedColumnSet($"{AxisTypeQuantity}{AxisTypeTime}", $"{AxisTypeQuantity}")];

    public static ImmutableArray<ExpectedColumnSet> BarChart =
        [new ExpectedColumnSet($"{AxisTypeQuantity}{AxisTypeTime}", $"{AxisTypeNominal}{AxisTypeOrdinal}")];

    public static ImmutableArray<ExpectedColumnSet> ColumnChart =
    [
        new ExpectedColumnSet($"{AxisTypeTime}{AxisTypeOrdinal}", $"{AxisTypeQuantity}{AxisTypeTime}")
    ];

    public static ImmutableArray<ExpectedColumnSet> LadderChart =
        [new ExpectedColumnSet($"{AxisTypeTime}", $"{AxisTypeTime}")];
}