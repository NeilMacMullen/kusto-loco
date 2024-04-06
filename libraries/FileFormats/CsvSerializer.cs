using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using KustoLoco.Core;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Util;
using NLog;

namespace KustoLoco.FileFormats;

public class CsvSerializer : ITableSerializer
{
    private readonly CsvConfiguration _config;
    
    public  CsvSerializer(CsvConfiguration config)
    {
       _config = config;
      
    }

    public static readonly CsvSerializer Default = new CsvSerializer(new CsvConfiguration(CultureInfo.InvariantCulture));
    public static readonly CsvSerializer Tsv = new CsvSerializer(new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = "\t"
    });


    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    private ITableSource Load(TextReader reader, string tableName, IProgress<string> progressReporter)
    {


       
        var csv = new CsvReader(reader, _config);
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
                progressReporter.Report($"{rowCount} records read");
        }

        var tableBuilder = TableBuilder.CreateEmpty(tableName, rowCount);
        for (var i = 0; i < keys.Length; i++)
        {
            tableBuilder.WithColumn(keys[i], builders[i].ToColumn());
        }

        var tableSource = tableBuilder.ToTableSource();
        progressReporter.Report("Loaded");
        return tableSource;

    }


    public  ITableSource Load(string filename,string tableName, IProgress<string> progressReporter)
    {
        using TextReader fileReader = new StreamReader(filename);
        return Load(fileReader,tableName,progressReporter);
    }


    public  ITableSource LoadFromString(string csv, string tableName)
    {
        var reader = new StringReader(csv.Trim());
        var table =Load(reader, tableName,new NullProgressReporter());
        return table;
    }


    public  void WriteToCsvStream(KustoQueryResult result, int max, bool skipHeader, TextWriter writer)
    {
        using var csv = new CsvWriter(writer, _config);
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

    private  void WriteToCsv(string path, KustoQueryResult result)
    {
        using var writer = new StreamWriter(path);
        WriteToCsvStream(result, int.MaxValue, false, writer);
    }

    public Task<TableLoadResult> LoadTable(string path, string tableName, IProgress<string> progressReporter)
    {
        try
        {
            var table = Load(path, tableName,progressReporter);
            table = TableBuilder.AutoInferColumnTypes(table, progressReporter);
            return Task.FromResult(TableLoadResult.Success(table));
        }
        catch (Exception e)
        {
            return Task.FromResult(TableLoadResult.Failure(e.Message));
        }
    }

    public bool RequiresTypeInference => true;

    public Task<TableSaveResult> SaveTable(string path,KustoQueryResult result, IProgress<string> progressReporter)
    {
        WriteToCsv(path,result);
        return Task.FromResult(TableSaveResult.Success());
    }
}
