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
        await using Stream fileStream = File.Create(path);
        return await SaveTable(fileStream, result);
    }

    public async Task<TableSaveResult> SaveTable(Stream fileStream, KustoQueryResult result)
    {
        var dataFields = result.ColumnDefinitions()
            .Select(columnDefinition =>
                new DataField(columnDefinition.Name, columnDefinition.UnderlyingType, true))
            .ToArray();
        if (!dataFields.Any())
        {
            _console.Warn("No columns in result - empty file/stream written");
            return TableSaveResult.Success();
        }

        var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());


        await using var writer = await ParquetWriter.CreateAsync(schema, fileStream);
        using var groupWriter = writer.CreateRowGroup();

        foreach (var columnDefinition in result
                     .ColumnDefinitions())
        {
            _console.ShowProgress($"Writing column {columnDefinition.Name}...");
            var dataColumn = new DataColumn(
                dataFields[columnDefinition.Index],
                CreateArrayFromRawObjects(columnDefinition, result)
            );
            await groupWriter.WriteColumnAsync(dataColumn);
        }

        _console.CompleteProgress("");
        return TableSaveResult.Success();
    }

    public async Task<TableLoadResult> LoadTable(Stream fileStream, string tableName)
    {
        if (fileStream.Length == 0)
            return TableLoadResult.Success(InMemoryTableSource.Empty);
        using var fileReader = await ParquetReader.CreateAsync(fileStream);
        var rowGroupCount = fileReader.RowGroupCount;
        //is it possible to have a parquet file with no row groups?
        //seems unlikely so treat it as fatal error.
        if (rowGroupCount == 0)
            return TableLoadResult.Failure("No row groups in file");

        var totalRows = 0;
        BaseColumnBuilder[] columnBuilders = [];

        for (var rowGroupIndex = 0; rowGroupIndex < rowGroupCount; rowGroupIndex++)
        {
            var rowGroup = await fileReader.ReadEntireRowGroupAsync(rowGroupIndex);

            //populate column builders if this is the first row group
            if (!columnBuilders.Any())
                columnBuilders = rowGroup
                    .Select(dataColumn => ColumnHelpers.CreateBuilder(dataColumn.Field.ClrType, dataColumn.Field.Name))
                    .ToArray();

            //add data from this row group
            var columnIndex = 0;
            foreach (var column in rowGroup)
            {
                //lookup the builder for this column
                var builderInfo = columnBuilders[columnIndex];
                builderInfo.AddCapacity(column.Data.Length);
                _console.ShowProgress(
                    $"Reading column {builderInfo.Name} from rowGroup {rowGroupIndex}");
                //TODO - surely there is a more efficient way to do this by wrapping the original data?
                foreach (var cellData in column.Data)
                    builderInfo.Add(cellData);
                columnIndex++;
            }

            totalRows += rowGroup.GetLength(0);
        }

        //having read all the data, we can now create the table
        foreach (var colBuilder in columnBuilders)
            colBuilder.TrimExcess();

        var tableBuilder = TableBuilder.CreateEmpty(tableName, totalRows);
        foreach (var colBuilder in columnBuilders)
            tableBuilder = tableBuilder.WithColumn(colBuilder.Name, colBuilder.ToColumn());

        _console.CompleteProgress("");
        return TableLoadResult.Success(tableBuilder.ToTableSource());
    }

    public async Task<TableLoadResult> LoadTable(string path, string tableName)
    {
        await using var fileStream = File.OpenRead(path);
        return await LoadTable(fileStream, tableName);
    }

    private static Array CreateArrayFromRawObjects(ColumnResult r, KustoQueryResult res)
    {
        //TODO - it feels like this could be done more efficiently
        var builder = ColumnHelpers.CreateBuilder(r.UnderlyingType, string.Empty);
        foreach (var cellData in res.EnumerateColumnData(r))
            builder.Add(cellData);
        return builder.GetDataAsArray();
    }
}
