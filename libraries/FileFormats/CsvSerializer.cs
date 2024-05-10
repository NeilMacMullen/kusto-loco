using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using KustoLoco.Core;
using KustoLoco.Core.Util;
using NLog;

namespace KustoLoco.FileFormats;

public class CsvSerializer : ITableSerializer
{
    public static readonly CsvSerializer Default = new(new CsvConfiguration(CultureInfo.InvariantCulture));

    public static readonly CsvSerializer Tsv = new(new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = "\t"
    });


    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly CsvConfiguration _config;

    public CsvSerializer(CsvConfiguration config)
    {
        _config = config;
    }

    public Task<TableLoadResult> LoadTable(string path, string tableName, IProgress<string> progressReporter,
        KustoSettings settings)
    {
        try
        {
            var table = Load(path, tableName, progressReporter, settings);
            if (!settings.Get(CsvSerializerSettings.SkipTypeInference,false))
                table = TableBuilder.AutoInferColumnTypes(table, progressReporter);

            return Task.FromResult(TableLoadResult.Success(table));
        }
        catch (Exception e)
        {
            return Task.FromResult(TableLoadResult.Failure(e.Message));
        }
    }

    public Task<TableSaveResult> SaveTable(string path, KustoQueryResult result, IProgress<string> progressReporter)
    {
        WriteToCsv(path, result);
        return Task.FromResult(TableSaveResult.Success());
    }


    private ITableSource Load(TextReader reader, string tableName, IProgress<string> progressReporter,
        KustoSettings settings)
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
            var IsTrimRequired = settings.Get(CsvSerializerSettings.TrimCells, true);
            string TrimIfRequired(string s) => IsTrimRequired ? s.Trim() : s;    
            for (var i = 0; i < keys.Length; i++) builders[i].Add(TrimIfRequired(csv.GetField<string>(i)));

            rowCount++;
            if (rowCount % 100_000 == 0)
                progressReporter.Report($"{rowCount} records read");
        }

        var tableBuilder = TableBuilder.CreateEmpty(tableName, rowCount);
        for (var i = 0; i < keys.Length; i++) tableBuilder.WithColumn(keys[i], builders[i].ToColumn());

        var tableSource = tableBuilder.ToTableSource();
        progressReporter.Report("Loaded");
        return tableSource;
    }


    public ITableSource Load(string filename, string tableName, IProgress<string> progressReporter,
        KustoSettings settings)
    {
        using TextReader fileReader = new StreamReader(filename);
        return Load(fileReader, tableName, progressReporter, settings);
    }


    public ITableSource LoadFromString(string csv, string tableName, KustoSettings settings)
    {
        var reader = new StringReader(csv.Trim());
        var table = Load(reader, tableName, new NullProgressReporter(), settings);
        return table;
    }


    public void WriteToCsvStream(KustoQueryResult result, int max, bool skipHeader, TextWriter writer)
    {
        using var csv = new CsvWriter(writer, _config);
        if (!skipHeader)
            foreach (var heading in result.ColumnNames())
                csv.WriteField(heading);
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

    private void WriteToCsv(string path, KustoQueryResult result)
    {
        using var writer = new StreamWriter(path);
        WriteToCsvStream(result, int.MaxValue, false, writer);
    }

    private static class CsvSerializerSettings
    {
        //TODO - source generation would allow much more flexibility for
        //self-describing settings  
        private const string prefix = "csv";
        private static string Setting(string setting) => $"{prefix}.{setting}";
        public static  string SkipTypeInference => Setting("skipTypeInference");
        public static string TrimCells => Setting("TrimCells");
    }
}



