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

    public static string RenderToHtml(string title, KustoQueryResult result) =>
        result.Visualization.ChartType.IsNotBlank()
            ? KustoToVegaChartType("title", result)
            : RenderToTable(result);

    public static string KustoToVegaChartType(string title, KustoQueryResult result)
    {
        var state = result.Visualization;
        return state.ChartType switch
        {
            /*
            "table" => VegaGenerator.LineChart,
            "list" => VegaGenerator.LineChart,
            "ladderchart" => VegaGenerator.LineChart,
            "timeline" => VegaGenerator.LineChart,
            "3Dchart" => VegaGenerator.LineChart,
            "card" => VegaGenerator.LineChart,
            "treemap" => VegaGenerator.LineChart,
            "plotly" => VegaGenerator.LineChart,
            "graph " => VegaGenerator.LineChart,
              "timepivot" => VegaGenerator.LineChart,
            */

            "barchart" => RenderToChart(title, BarChart, result, NoOp),
            "linechart" => RenderToChart(title, LineChart, result, NoOp),
            "piechart" => RenderToChart(title, PieChart, result, MakePieChart),
            "areachart" => RenderToChart(title, AreaChart, result, NoOp),
            "stackedareachart" => RenderToChart(title, AreaChart, result, MakeStacked),

            "ladderchart" => RenderToChart(title, BarChart, result, MakeTimeLineChart),

            /*
            "piechart" => VegaGenerator.PieChart,

            "timechart" => VegaGenerator.LineChart,
            "linechart" => VegaGenerator.LineChart,
            "anomalychart" => VegaGenerator.LineChart,
            "pivotchart" => VegaGenerator.LineChart,
            "areachart" => VegaGenerator.AreaChart,
            "stackedareachart" => VegaGenerator.AreaChart,
            "scatterchart" => VegaGenerator.LineChart,

            "columnchart" => VegaGenerator.BarChart,
            */

            _ => RenderToChart(title, InferChartTypeFromResult(result), result, NoOp)
        };
    }

    public static void NoOp(JObjectBuilder o)
    {
    }

    public static void NoOp(VegaChart o)
    {
    }

    public static string RenderToChart(string title, string vegaType, KustoQueryResult result,
        Action<JObjectBuilder> jmutate)
    {
        if (result.Height == 0)
            return result.Error;
        var b = RenderToJObjectBuilder(vegaType, result, jmutate);
        return VegaMaker.MakeHtml(title, b.Serialize());
    }


    public static void MakePieChart(JObjectBuilder b)
    {
        b.Set("mark.type", PieChart);
        b.Move("encoding.y", "encoding.theta");
        b.Set("encoding.color.type", AxisTypeNominal);
        b.Copy("encoding.x.field", "encoding.color.field");
        b.Remove("encoding.y");
        b.Remove("encoding.x");
    }


    public static void MakeTimeLineChart(JObjectBuilder b)
    {
        b.Set("mark.type", BarChart);
        b.Move("encoding.y", "encoding.x2");

        b.Set("encoding.y.type", AxisTypeOrdinal);
        b.Copy("encoding.color.title", "encoding.y.title");
        b.Copy("encoding.color.field", "encoding.y.field");
        b.Set("config.legend.disable", true);
    }

    public static void MakeStacked(JObjectBuilder b)
    {
        b.Set("encoding.y.aggregate", "sum");
    }

    public static string InferChartTypeFromResult(KustoQueryResult result)
    {
        var headers = result.ColumnDefinitions();
        var types = headers.Select(h => h.UnderlyingType).ToArray();

        return InferChartTypeFromAxisTypes(types.Select(InferSuitableAxisType).ToArray());
    }

    public static JObjectBuilder RenderToJObjectBuilder(string chartType,
        KustoQueryResult result,
        Action<JObjectBuilder> jmutate)
    {
        var headers = result.ColumnDefinitions();
        var types = headers.Select(h => h.UnderlyingType).ToArray();


        var color = new ColumnAndName(string.Empty, string.Empty);
        if (headers.Length > 2) color = new ColumnAndName(headers[2].Name, headers[2].Name);

        var spec = Spec(
            chartType,
            InferSuitableAxisType(types[0]),
            InferSuitableAxisType(types[1]),
            new ColumnAndName(headers.First().Name, headers.First().Name),
            new ColumnAndName(headers[1].Name, headers[1].Name),
            color
        );


        var b = JObjectBuilder.FromObject(spec);
        var rows = result.AsOrderedDictionarySet();

        b.Set("data.values", rows);
        jmutate(b);

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