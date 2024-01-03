using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using CsvHelper;
using Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace KustoSupport;

public static class KustoFormatter
{
    public static string ObjectToKustoString(object? o)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return o switch
        {
            null => string.Empty,
            DateTime d => d.ToString("u"),
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


        string JoinToLine(IEnumerable<string> columns) => columns.JoinString(" | ");
    }

    public static string WriteToCsvString(KustoQueryResult result, int max, bool skipHeaders) =>
        WriteToCsvString(result.AsOrderedDictionarySet(), max, skipHeaders);

    public static string WriteToCsvString(IReadOnlyCollection<OrderedDictionary> dictionaries, int max, bool skipHeader)
    {
        var headers = dictionaries.First().Cast<DictionaryEntry>().Select(de => de.Key.ToString()).ToArray();
        var writer = new StringWriter();
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            if (!skipHeader)
            {
                foreach (var heading in headers)
                {
                    csv.WriteField(heading);
                }
            }

            csv.NextRecord();

            foreach (var item in dictionaries.Take(max))
            {
                foreach (var heading in headers)
                {
                    csv.WriteField(item[heading]);
                }

                csv.NextRecord();
            }
        }

        return writer.ToString();
    }

    public static void WriteToCsv(string path, KustoQueryResult result)
    {
        var str = WriteToCsvString(result, int.MaxValue, false);
        File.WriteAllText(path, str);
    }
}