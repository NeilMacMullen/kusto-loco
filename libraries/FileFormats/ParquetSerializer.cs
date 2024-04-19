using KustoLoco.Core;
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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public async Task<TableLoadResult> LoadTable(string path, string tableName, IProgress<string> progressReporter, KustoSettings settings)
    {
        var table = await LoadFromFile(path, tableName, progressReporter);
        return TableLoadResult.Success(table);
    }

    public bool RequiresTypeInference { get; } = false;

    public async Task<TableSaveResult> SaveTable(string path, KustoQueryResult result,
        IProgress<string> progressReporter)
    {
        await Save(path, result);
        return TableSaveResult.Success();
    }

    public static async Task Save(string path, KustoQueryResult result)
    {
        await using Stream fs = File.OpenWrite(path);
        await SaveToStream(fs, result);
    }

    private static Array CreateArrayFromRawObjects(ColumnResult r, KustoQueryResult res)
    {
        //TODO - it feels like this could be done more efficiently
        var builder = ColumnHelpers.CreateBuilder(r.UnderlyingType);
        foreach (var o in res.EnumerateColumnData(r))
            builder.Add(o);
        return builder.GetDataAsArray();
    }

    public static async Task SaveToStream(Stream fs, KustoQueryResult result)
    {
        var dataFields = result.ColumnDefinitions()
            .Select(col =>
                new DataField(col.Name, col.UnderlyingType, true))
            .ToArray();
        var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());


        using var writer = await ParquetWriter.CreateAsync(schema, fs);
        using var groupWriter = writer.CreateRowGroup();

        foreach (var col in result
                     .ColumnDefinitions())
        {
            var dataColumn = new DataColumn(
                dataFields[col.Index],
                CreateArrayFromRawObjects(col, result)
            );
            await groupWriter.WriteColumnAsync(dataColumn);
        }
    }

    public static async Task<ITableSource> LoadFromFile(string path, string tableName,
        IProgress<string> progressReporter)
    {
        await using var fs = File.OpenRead(path);
        using var reader = await ParquetReader.CreateAsync(fs);
        var rg = await reader.ReadEntireRowGroupAsync();
        var tableBuilder = TableBuilder.CreateEmpty(tableName, rg.GetLength(0));
        foreach (var c in rg)
        {
            var type = c.Field.ClrType;
            progressReporter.Report($"reading column {c.Field.Name} of type {c.Field.Name}");
            //TODO - surely there is a more efficient way to do this by wrapping the original data?
            var colBuilder = ColumnHelpers.CreateBuilder(type);
            foreach (var o in c.Data)
                colBuilder.Add(o);

            tableBuilder.WithColumn(c.Field.Name, colBuilder.ToColumn());
        }

        return tableBuilder.ToTableSource();
    }
}
