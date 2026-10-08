using System.Globalization;
using System.Text;
using KustoLoco.Core;

namespace LokqlMcp;

public static class CsvFormatter
{
    public static string Format(IReadOnlyList<string> columns, IEnumerable<object?[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", columns.Select(Escape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(v => Escape(ToText(v)))));
        return sb.ToString();
    }

    public static string Format(KustoQueryResult result, int maxRows) =>
        Format(result.ColumnNames(), result.EnumerateRows(maxRows));

    private static string ToText(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string Escape(string s) =>
        s.IndexOfAny([',', '"', '\n', '\r']) >= 0
            ? "\"" + s.Replace("\"", "\"\"") + "\""
            : s;
}
