using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Extensions;
using KustoSupport;
using static System.Runtime.InteropServices.JavaScript.JSType;

#pragma warning disable CS8618, CS8600, CS8602, CS8604
public class KustoResultRenderer
{
    public static string RenderToTable(KustoQueryResult<OrderedDictionary> result)
    {
        var dictionaries = result.Results;
        var headers = dictionaries.First().Cast<DictionaryEntry>().Select(de => de.Key.ToString()).ToArray();

        var keyCount = Enumerable.Range(0, dictionaries.First().Count).ToArray();
        string SafeGet(IOrderedDictionary dict, int key) => dict[key]?.ToString() ?? string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("<table>");
        sb.AppendLine("<tr>");
        foreach (var h in headers)
            sb.AppendLine($"<th>{h}</th>");
        sb.AppendLine("</tr>");
        foreach (var d in dictionaries)
        {
            sb.AppendLine("<tr>");
            var line = keyCount.Select(c => $"<td>{SafeGet(d, c)}</td>").JoinAsLines();
            sb.AppendLine(line);
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        return sb.ToString();
    }

    public static string RenderToLineChart(string title,KustoQueryResult<OrderedDictionary> result)
    {
        var dictionaries = result.Results;
        var headers = dictionaries.First().Cast<DictionaryEntry>().Select(de => de.Key.ToString()).ToArray();

        var spec = VegaGenerator.Spec(
            VegaGenerator.LineChart,
            VegaGenerator.AxisTypeTime,
            VegaGenerator.AxisTypeQuantity,
            new ColumnAndName(headers.First(), headers.First()),
            new ColumnAndName(headers[1], headers[1]),
            new ColumnAndName(string.Empty, string.Empty));

        var b = JObjectBuilder.FromObject(spec);
        var row = 0;
        foreach (var d in dictionaries)
        {
            foreach (var h in headers)
            {
                b.Set($"data.values[{row}].{h}", d[h]);
            }

            row++;


        }

        b.Set("width", "container");
        b.Set("height", "container");


        return VegaMaker.MakeHtml(title,b.Serialize());
    }

}