using System.Text;
using NotNullStrings;

namespace Lokql.Engine;

public static class Tabulator
{
    public static string Tabulate<T> (IEnumerable<T> items,
        string headers,
        params Func<T, string>[] columns)
    {
        var headerArray = headers.Tokenize("|");
        var colWidths = columns
            .Select((c,idx) => items.Select(c).Append(headerArray[idx]).Max(s=>s.Length))
            .ToArray();
        var sb = new StringBuilder();
        var header = headerArray.Select((h,n)=> h.PadRight(colWidths[n]))
            .JoinString(" | ");
        sb.AppendLine(header);
        sb.AppendLine(headerArray.Select((_, n) => "".PadRight(colWidths[n],'-'))
            .JoinString("-|-")); ;
        foreach (var i in items)
        {
            var line =columns.Select((c,n)=> c(i).PadRight(colWidths[n]))
                .JoinString(" | ");
            sb.AppendLine(line);
        }
        return sb.ToString();
    }
}
