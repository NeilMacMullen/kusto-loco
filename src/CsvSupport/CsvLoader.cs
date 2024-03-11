using System.Globalization;
using BabyKusto.Core;
using BabyKusto.Core.Util;
using CsvHelper;
using KustoSupport;
using NLog;

namespace CsvSupport;

#pragma warning disable CS8602, CS8604, CS8600
public class CsvLoader : ITableLoader
{

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    private static ITableSource Load(TextReader reader, string tableName)
    {
        var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();
        var keys = csv.Context.Reader.HeaderRecord;
        var builders = keys
            .Select(_ => new ColumnBuilder<string>())
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

        var inferredColumns = builders
            .Select(b => ColumnTypeInferrer.AutoInfer(b.ToColumn()))
            .ToArray();

        var table = TableBuilder.CreateEmpty(tableName, rowCount);
        for (var i = 0; i < keys.Length; i++)
        {
            table.WithColumn(keys[i], inferredColumns[i]);
        }

        var t = table.ToTableSource();
        return t;

    }


    public static ITableSource Load(string filename,string tableName)
    {
        using TextReader fileReader = new StreamReader(filename);
        return Load(fileReader,tableName);
    }


    public static void LoadFromString(string csv, string tableName, KustoQueryContext context)
    {
        var reader = new StringReader(csv.Trim());
        var table =Load(reader, tableName);
        context.AddTable(table);
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

    public Task<TableLoadResult> LoadTable(string path, string tableName, IProgress<string> progressReporter)
    {
        try
        {
            var table = Load(path, tableName);
            return Task.FromResult(new TableLoadResult(table, string.Empty));
        }
        catch (Exception e)
        {
            return Task.FromResult(new TableLoadResult(NullTableSource.Instance, e.Message));
        }
    }

    public bool RequiresTypeInference { get; } = true;
}


public class ConsoleProgressReporter : IProgress<string>
{
    public void Report(string value)
    {
        Console.WriteLine(value);
    }
}

public class NullProgressReporter : IProgress<string>
{
    public void Report(string value)
    {
       
    }
}

