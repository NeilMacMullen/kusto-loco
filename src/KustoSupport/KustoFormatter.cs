using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Extensions;

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace KustoSupport;

public static class KustoFormatter
{
    public static string Tabulate(IReadOnlyCollection<OrderedDictionary> dictionaries)
    {
        var sb = new StringBuilder();
        if (!dictionaries.Any())
        {
            sb.AppendLine("No results");
        }

        var headers = dictionaries.First().Cast<DictionaryEntry>().Select(de => de.Key.ToString()).ToArray();

        var headerLengths = headers.Select(h => h.Length).ToArray();

        var keyCount = Enumerable.Range(0, dictionaries.First().Count).ToArray();
        var maxColumnSizes = keyCount
            .ToDictionary(i => i, MaxColumnSize);

        var headerLine = JoinToLine(headers.Select(PadForColumn));
        sb.AppendLine(headerLine);
        sb.AppendLine(keyCount.Select(hl => "".PadRight(MaxColumnSize(hl), '-')).JoinString("-+-"));
        foreach (var d in dictionaries)
        {
            var line = JoinToLine(keyCount.Select(c => PadForColumn(SafeGet(d, c), c)));
            sb.AppendLine(line);
        }

        return sb.ToString();

        string PadForColumn(string s, int c) => s.PadRight(maxColumnSizes[c]);

        string JoinToLine(IEnumerable<string> columns) => columns.JoinString(" | ");

        string SafeGet(IOrderedDictionary dict, int key) => dict[key]?.ToString() ?? string.Empty;

        int MaxColumnSize(int key)
            => dictionaries.Select(d => SafeGet(d, key).Length)
                .Append(headerLengths[key])
                .Max();
    }
}