using System.Globalization;
using BabyKusto.Core.Util;
using CsvHelper;
using KustoSupport;
using NLog;

namespace CsvSupport;

#pragma warning disable CS8602, CS8604, CS8600
public static class CsvLoader
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static void Load(TextReader reader, KustoQueryContext context, string tableName)
    {
        var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        var keys = csv.Context.Reader.HeaderRecord;
        var builders = keys.Select(_ => new ColumnBuilder<string>())
            .ToArray();
        var rowCount = 0;
        while (csv.Read())
        {
            for (var i = 0; i < keys.Length; i++)
            {
                builders[i].Add(csv.GetField<string>(i));
            }

            rowCount++;
            if (rowCount % 100_000 == 0)
                Logger.Info($"{rowCount} records read");
        }

        var inferredColumns = builders.Select(b =>
            ColumnTypeInferencer.AutoInfer(b.ToColumn())).ToArray();

        var table = TableBuilder.CreateEmpty(tableName, rowCount);
        for (var i = 0; i < keys.Length; i++)
        {
            table.WithColumn(keys[i], inferredColumns[i]);
        }

        var t = table.ToTableSource();
        context.AddTable(t);
    }


    public static void Load(string filename, KustoQueryContext context, string tableName)
    {
        using TextReader fileReader = new StreamReader(filename);
        Load(fileReader, context, tableName);
    }


    public static void LoadFromString(string csv, string tableName, KustoQueryContext context)
    {
        var reader = new StringReader(csv.Trim());
        Load(reader, context, tableName);
    }


    public static void WriteToCsvStream(KustoQueryResult result, int max, bool skipHeader, TextWriter writer)
    {
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        if (!skipHeader)
        {
            foreach (var heading in result.ColumnNames())
            {
                csv.WriteField(heading);
            }
        }

        csv.NextRecord();

        foreach (var r in result.EnumerateRows())
        {
            foreach (var cell in r)
            {
                var toPrint = cell is DateTime dt
                    ? dt.ToString("o", CultureInfo.InvariantCulture)
                    : Convert.ToString(cell, CultureInfo.InvariantCulture);
                csv.WriteField(toPrint);
            }

            csv.NextRecord();
        }
    }

    public static void WriteToCsv(string path, KustoQueryResult result)
    {
        using var writer = new StreamWriter(path);
        WriteToCsvStream(result, int.MaxValue, false, writer);
    }
}