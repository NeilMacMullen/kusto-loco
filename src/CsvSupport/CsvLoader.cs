using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using CsvHelper;
using KustoSupport;
using NLog;

namespace CsvSupport;

#pragma warning disable CS8602, CS8604, CS8600
public static class CsvLoader
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static List<OrderedDictionary> LoadAsOrderedDictionary(TextReader reader)
    {
        var records = new List<OrderedDictionary>();

        var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        var keys = csv.Context.Reader.HeaderRecord;
        var c = 0;
        while (csv.Read())
        {
            var dict = new OrderedDictionary();
            foreach (var header in keys)
                dict[header] = csv.GetField<string>(header);
            records.Add(dict);
            c++;
            if (c % 100_000 == 0)
                Logger.Info($"{c} records read");
        }


        InferTypes(records.ToArray());
        return records;
    }

    public static void Load(TextReader reader, KustoQueryContext context, string tableName)
    {
        var records = LoadAsOrderedDictionary(reader);

        context
            .AddTable(TableBuilder
                .FromOrderedDictionarySet(tableName,
                    records));
    }

    public static void Load(string filename, KustoQueryContext context, string tableName)
    {
        using TextReader fileReader = new StreamReader(filename);
        Load(fileReader, context, tableName);
    }


    private static void InferTypes(
        OrderedDictionary[] dictionaries)
    {
        var headers = dictionaries.First().Cast<DictionaryEntry>().Select(de => de.Key.ToString()).ToArray();
        var columnCount = dictionaries.First().Count;
        var rowCount = dictionaries.Length;

        var typeTriers = new Func<string, (bool, object)>[]
        {
            s => (int.TryParse(s, out var i), i),
            s => (long.TryParse(s, out var i), i),
            s => (double.TryParse(s, out var i), i),
            s => (DateTime.TryParse(s, out var i), i),
            s => (Guid.TryParse(s, out var i), i),
            s => (TimeSpan.TryParse(s, out var i), i),
            s => (bool.TryParse(s, out var i), i),
        };

        for (var c = 0; c < columnCount; c++)
        {
            var column = dictionaries.Select(d => (string)d[c]).ToArray();

            var transformed = new object?[rowCount];
            var inferredType = false;
            foreach (var typeParser in typeTriers)
            {
                var processedAll = true;
                for (var i = 0; i < rowCount; i++)
                {
                    var cell = column[i];
                    //blank cells tell us nothing about type since data may
                    //be missing
                    if (string.IsNullOrWhiteSpace(cell))
                    {
                        transformed[i] = null;
                        continue;
                    }

                    var (parsed, val) = typeParser(cell);
                    if (parsed)
                        transformed[i] = val;
                    else
                    {
                        processedAll = false;
                        break;
                    }
                }

                if (!processedAll) continue;
                inferredType = true;
                break;
            }

            if (!inferredType)
                continue;
            for (var r = 0; r < rowCount; r++)
                dictionaries[r][headers[c]] = transformed[r];
        }
    }

    public static void LoadFromString(string csv, string tableName, KustoQueryContext context)
    {
        var reader = new StringReader(csv.Trim());
        Load(reader, context, tableName);
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