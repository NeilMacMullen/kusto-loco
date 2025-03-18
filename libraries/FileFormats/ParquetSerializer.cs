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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public ParquetSerializer(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
    }

    public static ParquetSerializer Default => new(new KustoSettingsProvider(), new NullConsole());


    public async Task<TableSaveResult> SaveTable(string path, KustoQueryResult result)
    {
        await using Stream fs = File.Create(path);
        return await SaveTable(fs, result);
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
        using var fileReader = await ParquetReader.CreateAsync(fs);
        var rowGroupCount = fileReader.RowGroupCount;
        //is it possible to have a parquet file with no rowgroups?
        //seems unlikely so treat it as fatal error.
        if (rowGroupCount == 0)
            return TableLoadResult.Failure("No row groups in file");

        var totalRows = 0;
        BuilderInfo[] columnBuilders = [];

        for (var rowGroupIndex = 0; rowGroupIndex < rowGroupCount; rowGroupIndex++)
        {
            var rowGroup = await fileReader.ReadEntireRowGroupAsync(rowGroupIndex);

            //populate column builders if this is the first row group
            if (!columnBuilders.Any())
            {
                var cols = new List<BuilderInfo>();
                foreach (var c in rowGroup)
                {
                    var type = c.Field.ClrType;
                    var colBuilder = ColumnHelpers.CreateBuilder(type);
                    cols.Add(new BuilderInfo(c.Field.Name, type, colBuilder));
                }

                columnBuilders = cols.ToArray();
            }

            //add data from this rowgroup
            var columnIndex = 0;
            foreach (var column in rowGroup)
            {
                var builderInfo = columnBuilders[columnIndex];
                _console.ShowProgress(
                    $"Reading column {builderInfo.Name} of type {builderInfo.Type.Name} from rowGroup {rowGroupIndex}");
                //TODO - surely there is a more efficient way to do this by wrapping the original data?
                foreach (var o in column.Data)
                    builderInfo.Builder.Add(o);
                columnIndex++;
            }

            totalRows += rowGroup.GetLength(0);
        }

        //having read all the data, we can now create the table
        var tableBuilder = TableBuilder.CreateEmpty(tableName, totalRows);
        foreach (var colBuilder in columnBuilders)
            tableBuilder = tableBuilder.WithColumn(colBuilder.Name, colBuilder.Builder.ToColumn());

        _console.CompleteProgress("");
        return TableLoadResult.Success(tableBuilder.ToTableSource());
    }

    public async Task<TableLoadResult> LoadTable(string path, string tableName)
    {
        await using var fs = File.OpenRead(path);
        return await LoadTable(fs, tableName);
    }

    private static Array CreateArrayFromRawObjects(ColumnResult r, KustoQueryResult res)
    {
        //TODO - it feels like this could be done more efficiently
        var builder = ColumnHelpers.CreateBuilder(r.UnderlyingType);
        foreach (var o in res.EnumerateColumnData(r))
            builder.Add(o);
        return builder.GetDataAsArray();
    }

    private readonly record struct BuilderInfo(string Name, Type Type, BaseColumnBuilder Builder);
}
