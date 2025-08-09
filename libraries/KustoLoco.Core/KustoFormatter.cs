using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using NotNullStrings;

namespace KustoLoco.Core;

public static class KustoFormatter
{
    public static string ObjectToKustoString(object? o)
    {

        return o switch
        {
            null => "<null>",
            DateTime d => d.Kind == DateTimeKind.Local
                ? d.ToString("yyyy-MM-dd HH:mm:ss.ffff")
                : d.ToString("u"),
            _ => o.ToString()!
        };
    }

    /// <summary>
    ///     rather roundabout way of turning ordered dictionaries into tabulated text
    /// </summary>
    /// <param name="dictionaries"></param>
    /// <returns></returns>
    public static string Tabulate(IReadOnlyCollection<OrderedDictionary> dictionaries)
    {
        var table = TableBuilder.FromOrderedDictionarySet(string.Empty, dictionaries);
        var source = table.ToTableSource() as InMemoryTableSource 
                  ?? throw new NotImplementedException("TableBuilder is currently assumed to return an InMemoryTableSource");
        var result = new KustoQueryResult(string.Empty, source, VisualizationState.Empty, TimeSpan.Zero, string.Empty);
        return Tabulate(result);
    }

    public static string Tabulate(KustoQueryResult result, int max = int.MaxValue)
        => Tabulate(result, new DisplayPreferences(int.MaxValue, 0, max));

    public static string Tabulate(KustoQueryResult result, DisplayPreferences preferences)
    {
        if (result.RowCount == 0)
            return "no results";
        var columns = result.ColumnDefinitions();
        var sb = new StringBuilder();

        var max = preferences.Length;

        var displayHeight = Math.Min(max, result.RowCount);
        var maxColumnWidth = preferences.ScreenWidth;
        var screenWidth = Math.Max(10, preferences.ScreenWidth);


        string[] MakeStringColumn(ColumnResult c)
            => new[] { c.Name }
                .Concat(result.EnumerateColumnData(c)
                    .Skip(preferences.StartOffset)
                    .Take(max))
                .Select(ObjectToKustoString)
                .ToArray();

        string[] PadToMax(string[] a)
        {
            var maxWidth = a.Select(s => s.Length).Max();

            return a.Select(s => s.PadRight(maxWidth))
                .Select(s => s[..Math.Min(s.Length, maxColumnWidth)])
                .ToArray();
        }

        var cells = columns
            .Select(MakeStringColumn)
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


        string JoinToLine(IEnumerable<string> cols)
        {
            var naive = cols.JoinString(" | ");
            if (naive.Length > screenWidth)
            {
                naive = naive[..(screenWidth - 3)] + "...";
            }

            return naive;
        }
    }

    public readonly record struct DisplayPreferences(int ScreenWidth, int StartOffset, int Length);
}
