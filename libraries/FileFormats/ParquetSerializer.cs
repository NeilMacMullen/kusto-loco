using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using NLog;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace KustoLoco.FileFormats;

/// <summary>
///     Basic support for saving and loading parquet files.
/// </summary>
/// <remarks>
///     Could be extended to support chunks, indices and other parquet features.
/// </remarks>
public class ParquetSerializer : ITableSerializer
{

    public ParquetSerializer(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
    }
    public static ParquetSerializer Default => new(new KustoSettingsProvider(), new NullConsole());

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly KustoSettingsProvider _settings;
    private readonly IKustoConsole _console;

 
    public async Task<TableSaveResult> SaveTable(string path, KustoQueryResult result)
    {
        await using Stream fs = File.OpenWrite(path);
        return await SaveTable(fs, result);
    }

    private static Array CreateArrayFromRawObjects(ColumnResult r, KustoQueryResult res)
    {
        //TODO - it feels like this could be done more efficiently
        var builder = ColumnHelpers.CreateBuilder(r.UnderlyingType);
        foreach (var o in res.EnumerateColumnData(r))
            builder.Add(o);
        return builder.GetDataAsArray();
    }

    public async Task<TableSaveResult> SaveTable(Stream fs, KustoQueryResult result)
    {
        var dataFields = result.ColumnDefinitions()
            .Select(col =>
                new DataField(col.Name, col.UnderlyingType, true))
            .ToArray();
        if (!dataFields.Any())
        {
            _console.Warn("No columns in result - empty file/stream written");
            return TableSaveResult.Success();
        }
        var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());


        using var writer = await ParquetWriter.CreateAsync(schema, fs);
        using var groupWriter = writer.CreateRowGroup();

        foreach (var col in result
                     .ColumnDefinitions())
        {
            _console.ShowProgress($"Writing column {col.Name}...");
            var dataColumn = new DataColumn(
                dataFields[col.Index],
                CreateArrayFromRawObjects(col, result)
            );
            await groupWriter.WriteColumnAsync(dataColumn);
        }
        _console.CompleteProgress("");
        return TableSaveResult.Success();
    }


    public async Task<TableLoadResult> LoadTable(Stream fs, string tableName)
    {
        if (fs.Length == 0)
            return TableLoadResult.Success(InMemoryTableSource.Empty);
        using var reader = await ParquetReader.CreateAsync(fs);
        var rg = await reader.ReadEntireRowGroupAsync();
        var tableBuilder = TableBuilder.CreateEmpty(tableName, rg.GetLength(0));
        foreach (var c in rg)
        {
            var type = c.Field.ClrType;
            _console.ShowProgress($"Reading column {c.Field.Name} of type {type.Name}");
            //TODO - surely there is a more efficient way to do this by wrapping the original data?
            var colBuilder = ColumnHelpers.CreateBuilder(type);
            foreach (var o in c.Data)
                colBuilder.Add(o);

            tableBuilder.WithColumn(c.Field.Name, colBuilder.ToColumn());
        }
        _console.CompleteProgress("");
        return TableLoadResult.Success(tableBuilder.ToTableSource());
    }

    public async Task<TableLoadResult> LoadTable(string path, string tableName)
    {
        await using var fs = File.OpenRead(path);
        return await LoadTable(fs, tableName);

    }
}
