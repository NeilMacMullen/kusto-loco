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

    public static string RenderToHmtl(string title, KustoQueryResult result) =>
        result.Visualization.ChartType.IsNotBlank()
            ? RenderToLineChart("title", result)
            : RenderToTable(result);

    public static string RenderToLineChart(string title, KustoQueryResult result)
    {
        if (result.Height == 0)
            return result.Error;
        var headers = result.ColumnDefinitions();
        var types = headers.Select(h => h.Type).ToArray();

        string AxisType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return AxisType(Nullable.GetUnderlyingType(t));
            }

            var numericTypes = new[]
            {
                typeof(int), typeof(long), typeof(double), typeof(float),
            };
            if (numericTypes.Contains(t))
                return VegaGenerator.AxisTypeQuantity;

            if (t == typeof(DateTime))
                return VegaGenerator.AxisTypeTime;
            return VegaGenerator.AxisTypeOrdinal;
        }

        string ChartType(string[] axisTypes)
        {
            var x = axisTypes[0];
            var y = axisTypes.Skip(1).ToArray();
            if (x == VegaGenerator.AxisTypeOrdinal)
                return VegaGenerator.BarChart;
            return VegaGenerator.LineChart;
        }

        var chartType = ChartType(types.Select(AxisType).ToArray());

        var color = new ColumnAndName(string.Empty, string.Empty);
        if (headers.Length > 2) color = new ColumnAndName(headers[2].Name, headers[2].Name);

        var spec = VegaGenerator.Spec(
            chartType,
            AxisType(types[0]),
            AxisType(types[1]),
            new ColumnAndName(headers.First().Name, headers.First().Name),
            new ColumnAndName(headers[1].Name, headers[1].Name),
            color
        );

        var b = JObjectBuilder.FromObject(spec);
        var dicts = result.AsOrderedDictionarySet();
        /*
        for (var row = 0; row < result.Height; row++)
        {
            for (var col = 0; col < headers.Length; col++)
            {
                var h = headers[col];

                b.Set($"data.values[{row}].{h.Name}", result.Get(col, row));
            }
        }
        */

        b.Set("data.values", dicts);

        b.Set("width", "container");
        b.Set("height", "container");


        return VegaMaker.MakeHtml(title, b.Serialize());
    }
}