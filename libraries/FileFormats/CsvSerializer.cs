using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using NLog;

namespace KustoLoco.FileFormats;

public class CsvSerializer : ITableSerializer
{
    private readonly CsvConfiguration _config;
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public CsvSerializer(CsvConfiguration config, KustoSettingsProvider settings, IKustoConsole console)
    {
        _config = config;
        _settings = settings;
        _console = console;
        settings.Register(CsvSerializerSettings.SkipTypeInference, CsvSerializerSettings.TrimCells);
    }

    public Task<TableLoadResult> LoadTable(string path, string tableName)
    {
        try
        {
            var table = Load(path, tableName);
            if (!_settings.GetBool(CsvSerializerSettings.SkipTypeInference))
                table = TableBuilder.AutoInferColumnTypes(table, _console);

            return Task.FromResult(TableLoadResult.Success(table));
        }
        catch (Exception e)
        {
            return Task.FromResult(TableLoadResult.Failure(e.Message));
        }
    }

    public Task<TableSaveResult> SaveTable(string path, KustoQueryResult result)
    {
        WriteToCsv(path, result);
        return Task.FromResult(TableSaveResult.Success());
    }


    public static CsvSerializer Default(KustoSettingsProvider settings, IKustoConsole console)
    {
        return new CsvSerializer(new CsvConfiguration(CultureInfo.InvariantCulture), settings, console);
    }

    public static CsvSerializer Tsv(KustoSettingsProvider settings, IKustoConsole console)
    {
        return new CsvSerializer(new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t"
            }
            , settings, console);
    }

    private ITableSource Load(TextReader reader, string tableName)
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
            var IsTrimRequired = _settings.GetBool(CsvSerializerSettings.TrimCells);

            string TrimIfRequired(string s)
            {
                return IsTrimRequired ? s.Trim() : s;
            }

            for (var i = 0; i < keys.Length; i++) builders[i].Add(TrimIfRequired(csv.GetField<string>(i)));

            rowCount++;
            if (rowCount % 100_000 == 0)
                _console.ShowProgress($"loaded {rowCount} records");
        }
       
        var tableBuilder = TableBuilder.CreateEmpty(tableName, rowCount);
        for (var i = 0; i < keys.Length; i++) tableBuilder.WithColumn(keys[i], builders[i].ToColumn());

        var tableSource = tableBuilder.ToTableSource();
        _console.CompleteProgress($"loaded {rowCount} records");
        return tableSource;
    }


    public ITableSource Load(string filename, string tableName)
    {
        using TextReader fileReader = new StreamReader(filename);
        return Load(fileReader, tableName);
    }


    public ITableSource LoadFromString(string csv, string tableName)
    {
        var reader = new StringReader(csv.Trim());
        var table = Load(reader, tableName);
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
        private const string prefix = "csv";

        public static readonly KustoSettingDefinition SkipTypeInference = new(
            Setting("skipTypeInference"), "prevents conversion of string columns to types",
            "off",
            nameof(String));

        public static readonly KustoSettingDefinition TrimCells = new(Setting("TrimCells"),
            "Removes leading and trailing whitespace from string values", "true", nameof(Boolean));

        private static string Setting(string setting)
        {
            return $"{prefix}.{setting}";
        }
    }
}
