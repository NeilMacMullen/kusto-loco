using System.Text;
using Extensions;

#pragma warning disable CS8604 // Possible null reference argument.


namespace KustoSupport;

public static class KustoFormatter
{
    public static string ObjectToKustoString(object? o)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return o switch
        {
            null => string.Empty,
            DateTime d => d.Kind == DateTimeKind.Local
                ? d.ToString("yyyy-MM-dd HH:mm:ss.ffff")
                : d.ToString("u"),
            _ => o.ToString()
        };
#pragma warning restore CS8603 // Possible null reference return.
    }

    public static string Tabulate(KustoQueryResult result, int max = int.MaxValue)
    {
        if (result.Height == 0)
            return "no results";
        var columns = result.ColumnDefinitions();
        var sb = new StringBuilder();

        var displayHeight = Math.Min(max, result.Height);

        string[] MakeStringColumn(ColumnResult c)
            => new[] { c.Name }
                .Concat(result.EnumerateColumnData(c).Take(max))
                .Select(ObjectToKustoString)
                .ToArray();

        string[] PadToMax(string[] a)
        {
            var maxWidth = a.Select(s => s.Length).Max();
            return a.Select(s => s.PadRight(maxWidth)).ToArray();
        }

        var cells = columns.Select(MakeStringColumn)
            .Select(PadToMax)
            .ToArray();


        for (var r = 0; r <= displayHeight; r++)
        {
            var line = cells.Select(c => c[r]).ToArray();
            sb.AppendLine(JoinToLine(line));
            if (r == 0)
            {
                var dividerLine = line.Select(c => "".PadRight(c.Length, '-'));
                sb.AppendLine(JoinToLine(dividerLine));
            }
        }

        return sb.ToString();


        string JoinToLine(IEnumerable<string> cols) => cols.JoinString(" | ");
    }
}