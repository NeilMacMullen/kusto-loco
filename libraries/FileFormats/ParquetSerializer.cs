using System.Diagnostics;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
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

    public async Task SaveTestTable(Stream fileStream,int count)
    {
        var dataFields = new[] { new DataField("test", typeof(int), false) };
        var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());

        await using var writer = await ParquetWriter.CreateAsync(schema, fileStream);
        using var groupWriter = writer.CreateRowGroup();
        var arr = Enumerable.Range(0, 100).ToArray();
        var def = dataFields[0];
        var dataColumn = new DataColumn(def, arr);
        await groupWriter.WriteColumnAsync(dataColumn);
    }

    public const int Max_RowGroupSize = 10_000_000;


    private DataField Create(ColumnResult columnDefinition)
        =>
            columnDefinition.UnderlyingType == typeof(TimeSpan)
                ? new TimeSpanDataField(columnDefinition.Name, TimeSpanFormat.MicroSeconds, true)
                : new DataField(columnDefinition.Name, columnDefinition.UnderlyingType, true);

public async Task<TableSaveResult> SaveTable(Stream fileStream, KustoQueryResult result)
    {
        var dataFields = result.ColumnDefinitions()
            .Select(columnDefinition => Create(columnDefinition))
            .ToArray();
        if (!dataFields.Any())
        {
            _console.Warn("No columns in result - empty file/stream written");
            return TableSaveResult.Success();
        }

        var schema = new ParquetSchema(dataFields.Cast<Field>().ToArray());


        await using var writer = await ParquetWriter.CreateAsync(schema, fileStream);

        var offset = 0;
        while (offset < result.RowCount)
        {
            var length = Math.Min(Max_RowGroupSize, result.RowCount-offset);
            using var groupWriter = writer.CreateRowGroup();

            foreach (var columnDefinition in result
                         .ColumnDefinitions())
            {
                _console.ShowProgress($"Writing column {columnDefinition.Name} {offset} {length}...");
                var data = CreateArrayFromRawObjects(columnDefinition, result,offset,length);
                //todo we could optimise saving here by writing non-nullable types but
                //we'd need to know whether there are nulls before creating the schema
                //and that would probably require us to propagate this knowledge through
                //Columns
                //if (data.NoNulls)
                //    def = new DataField(def.Name, TypeMapping.UnderlyingType(def.ClrType));
                var arr = data.GetDataAsArray();
                var def = dataFields[columnDefinition.Index];
                var dataColumn = new DataColumn(def, arr);
                await groupWriter.WriteColumnAsync(dataColumn);
              
            }
            offset += length;
        }

        _console.CompleteProgress("");
        return TableSaveResult.Success();
    }

    public async Task<TableLoadResult> LoadTable(Stream fileStream, string tableName)
    {
        if (fileStream.Length == 0)
            return TableLoadResult.Success(InMemoryTableSource.Empty);
        _console.WriteLine("Reading stream...");
        using var fileReader = await ParquetReader.CreateAsync(fileStream);
        var rowGroupCount = fileReader.RowGroupCount;
        //KustoResult.Empty can produce this
        if (rowGroupCount == 0)
            return TableLoadResult.Success(InMemoryTableSource.Empty);

        var totalRows = 0;
        BaseColumnBuilder[] columnBuilders = [];
        for (var rowGroupIndex = 0; rowGroupIndex < rowGroupCount; rowGroupIndex++)
        {
            var rowGroup = await fileReader.ReadEntireRowGroupAsync(rowGroupIndex);

            var pqColumns = rowGroup.Select(d => d).ToArray();
            //populate column builders if this is the first row group
            if (!columnBuilders.Any())
                columnBuilders = rowGroup
                    .Select(dataColumn => ColumnHelpers.CreateBuilder(dataColumn.Field.ClrType, dataColumn.Field.Name))
                    .ToArray();
        
            //add data from this row group
            var columnIndex = 0;
            var stopwatch = Stopwatch.StartNew();
            var columnHeight = 0;
            foreach (var column in rowGroup)
            {
                var dataColumn = rowGroup[columnIndex];
                var builder = columnBuilders[columnIndex];
                _console.ShowProgress(
                    $"Column:{dataColumn.Field.Name} {rowGroupIndex+1}/{rowGroupCount}  (offset {totalRows})");
                foreach(var o in column.Data)
                    builder.Add(o);
                columnHeight = column.Data.Length;
                  columnIndex++;
            }

            totalRows += columnHeight;
        }

        //having read all the data, we can now create the table
       // foreach (var colBuilder in columnBuilders)
        //    colBuilder.TrimExcess();

        var tableBuilder = TableBuilder.CreateEmpty(tableName, totalRows);
        for (var i = 0; i < columnBuilders.Length; i++)
        {
            var builder = columnBuilders[i];
            tableBuilder = tableBuilder.WithColumn(builder.Name,builder.ToColumn());
        }   
        _console.CompleteProgress("");
        return TableLoadResult.Success(tableBuilder.ToTableSource());
    }

    public async Task<TableLoadResult> LoadTable(string path, string tableName)
    {
        await using var fileStream = File.OpenRead(path);
        return await LoadTable(fileStream, tableName);
    }

    private static INullableSet CreateArrayFromRawObjects(ColumnResult r, KustoQueryResult res,int start,int length)
    {
        var builder = NullableSetBuilderLocator.GetFixedNullableSetBuilderForType(r.UnderlyingType, length);
        foreach (var cellData in res.EnumerateColumnData(r).Skip(start).Take(length))
            builder.Add(cellData);
        var set= builder.ToINullableSet();
        return set;
    }
}
