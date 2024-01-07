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
        var rowCount = Math.Min(dictionaries.Length, 1000);

        var typeTriers = new Func<string, (bool, object)>[]
        {
            s => (int.TryParse(s, out var i), i),
            s => (double.TryParse(s, out var i), i),
            s => (DateTime.TryParse(s, out var i), i),
            s => (bool.TryParse(s, out var i), i),
        };

        for (var c = 0; c < columnCount; c++)
        {
            var column = dictionaries.Select(d => (string)d[c]).ToArray();

            var transformed = new object[rowCount];
            var inferredType = false;
            foreach (var type in typeTriers)
            {
                var processedAll = true;
                for (var i = 0; i < rowCount; i++)
                {
                    var (parsed, val) = type(column[i]);
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

            if (!inferredType) continue;
            for (var r = 0; r < rowCount; r++)
                dictionaries[r][headers[c]] = transformed[r];
        }
    }

    public static void LoadFromString(string csv, string tableName, KustoQueryContext context)
    {
        var reader = new StringReader(csv.Trim());
        Load(reader, context, tableName);
    }
}