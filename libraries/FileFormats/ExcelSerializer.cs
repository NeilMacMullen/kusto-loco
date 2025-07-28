using System.Data;
using System.Text;
using ClosedXML.Excel;
using ExcelDataReader;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using NotNullStrings;

namespace KustoLoco.FileFormats;

#pragma warning disable CS8604
public class ExcelSerializer : ITableSerializer
{
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public ExcelSerializer(KustoSettingsProvider settings, IKustoConsole console)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _settings = settings;
        _console = console;
    }

    public async Task<TableSaveResult> SaveTable(string path, KustoQueryResult result)
    {
        await using var stream = File.Create(path);
        return await SaveTable(stream, result);
    }

    /// <summary>
    ///     Loads a table from an Excel file
    /// </summary>
    /// <remarks>
    ///     Excel files can contain multiple tables, but this method will only load the first table in the file.
    /// </remarks>
    public Task<TableLoadResult> LoadTable(Stream stream, string tableName)
    {
        // Auto-detect format, supports:
        //  - Binary Excel files (2.0-2003 format; *.xls)
        //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet();
        if (result.Tables.Count == 0)
            return Task.FromResult(TableLoadResult.Failure("no data found in file"));
        var dataTable = result.Tables[0];
        var loadResult = CreateTableFromDataTable(tableName, dataTable);

        return Task.FromResult(loadResult);
    }

    public async Task<TableLoadResult> LoadTable(string filename, string tableName)
    {
        try
        {
            await using var fileReader = File.OpenRead(filename);
            return await LoadTable(fileReader, tableName);
        }
        catch (Exception e)
        {
            var message = e.Message;
            //add helpful hint fo common error
            if (message.Contains("because it is being used by another process"))
                message += " Is the file open in Excel or another program?";
            return TableLoadResult.Failure(message);
        }
    }

    public Task<TableSaveResult> SaveTable(Stream stream, KustoQueryResult result)
    {
        var dataTable = result.ToDataTable();
        using (var workbook = new XLWorkbook())
        {
            workbook.Worksheets.Add(dataTable, "Sheet1");
            workbook.SaveAs(stream);
        }
        return Task.FromResult(TableSaveResult.Success());
    }

    private TableLoadResult CreateTableFromDataTable(string tableName, DataTable dataTable)
    {
        var dr = DataTableReader.Create(dataTable)
            .Trim();

        if (dr.ColumnCount < 1 || dr.RowCount < 2)
            return TableLoadResult.Failure("no data found in table");

        var rowCount = dr.RowCount - 1;

        var keys = dr.GetRowItems(dr.GetRow(0)).Select(o => o?.ToString().NullToEmpty()).ToArray();

        var tableBuilder = TableBuilder.CreateEmpty(tableName, rowCount);
        for (var columnIndex = 0; columnIndex < dr.ColumnCount; columnIndex++)
        {
            var columnType = dr.GetColumnType(columnIndex);
            var colData = dr.GetColumnItems(columnIndex).Skip(1);
            BaseColumn column;
            if (columnType == typeof(object))
            {
                //'object' implies the column is a mix of types, so we need to
                //convert to string and then infer the type
                var colBuilder = new GenericColumnBuilderOfstring();
                foreach (var item in colData) colBuilder.Add(item?.ToString().NullToEmpty());
                column = ColumnTypeInferrer.AutoInfer(colBuilder.ToColumn());
            }
            else
            {
                var colBuilder = ColumnHelpers.CreateBuilder(columnType,String.Empty);
                foreach (var item in colData) colBuilder.Add(item is DBNull ? null : item);
                column = colBuilder.ToColumn();
            }

            tableBuilder.WithColumn(keys[columnIndex], column);
        }

        var tableSource = tableBuilder.ToTableSource();
        _console.CompleteProgress($"loaded {rowCount} records");
        return TableLoadResult.Success(tableSource);
    }

    public async Task<IReadOnlyCollection<TableLoadResult>> LoadAllTables(string filename, string prefix)
    {
        try
        {
            await using var stream = File.OpenRead(filename);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();
            var results = new List<TableLoadResult>();
            for (var i = 0; i < result.Tables.Count; i++)
            {
                var dataTable = result.Tables[i];
                var tableName = $"{prefix}_{dataTable.TableName}";
                results.Add(CreateTableFromDataTable($"{prefix}_{dataTable}", dataTable));
            }

            return results;
        }
        catch (Exception e)
        {
            var message = e.Message;
            //add helpful hint fo common error
            if (message.Contains("because it is being used by another process"))
                message += " Is the file open in Excel or another program?";
            return [TableLoadResult.Failure(message)];
        }
    }
}
