using System.Data;

namespace KustoLoco.FileFormats;

#pragma warning disable CS8604
/// <summary>
/// Manages DataTables
/// </summary>
/// <remarks>
/// Allows a virtual view onto a datatable , with the ability to trim rows and columns
/// This is useful when reading DataSets from Excel files, where there may be blank rows
/// and columns around the populated area.
/// </remarks>
public class DataTableReader
{
    public readonly int RowCount;
    private readonly int _startColumn;
    private readonly int _startRow;
    private readonly DataTable _table;
    public readonly int ColumnCount;

    private DataTableReader(DataTable table, int startRow, int rowCount, int startColumn, int columnCount)
    {
        _table = table;
        _startRow = startRow;
        RowCount = rowCount;
        _startColumn = startColumn;
        ColumnCount = columnCount;
    }

    public string TableName => _table.TableName;

    public static DataTableReader Create(DataTable table)
    {
        var numRows = table.Rows.Count;
        var numColumns = table.Columns.Count;
        return new DataTableReader(table, 0, numRows, 0, numColumns);
    }

    public static bool CellIsNullOrEmpty(object? x)
    {
        return
            x is DBNull ||
            x is null ||
            (x is string s && string.IsNullOrWhiteSpace(s));
    }

    public DataRow GetRow(int n)
    {
        return _table.Rows[n + _startRow];
    }

    private int GetRawColumnIndex(int n)
    {
        return n + _startColumn;
    }

    public IEnumerable<DataRow> GetRows()
    {
        var rows = Enumerable.Range(0, RowCount)
            .Select(GetRow).ToArray();
        return rows;
    }

    public IEnumerable<object?> GetRowItems(DataRow row)
    {
        var items = Enumerable.Range(0, ColumnCount)
            .Select(i => row[GetRawColumnIndex(i)])
            .ToArray();
        return items;
    }

    public IEnumerable<object?> GetColumnItems(int column)
    {
        var rawIndex = GetRawColumnIndex(column);
        var items = GetRows().Select(r => r[rawIndex])
            .ToArray();
        return items;
    }

    private bool IsRowBlank(int n)
    {
        return GetRowItems(GetRow(n))
            .All(CellIsNullOrEmpty);
    }

    private bool IsColumnBlank(int n)
    {
        return GetColumnItems(n)
            .All(CellIsNullOrEmpty);
    }


    public DataTableReader TrimRows()
    {
        var startRow = _startRow;
        var endRow = _startRow + RowCount - 1;
        while (startRow <= endRow && IsRowBlank(startRow)) startRow++;
        while (startRow <= endRow && IsRowBlank(endRow)) endRow--;
        var numRows = endRow - startRow + 1;
        return new DataTableReader(_table, startRow, numRows, _startColumn, ColumnCount);
    }

    public DataTableReader TrimColumns()
    {
        var startColumn = _startColumn;
        var endColumn = _startColumn + ColumnCount - 1;
        while (startColumn <= endColumn && IsColumnBlank(startColumn)) startColumn++;
        while (startColumn <= endColumn && IsColumnBlank(endColumn)) endColumn--;
        var columnCount = endColumn - startColumn + 1;
        return new DataTableReader(_table, _startRow, RowCount, startColumn, columnCount);
    }

    public DataTableReader Trim()
    {
        return TrimRows().TrimColumns();
    }

    public Type GetColumnType(int column)
    {
        var items = GetColumnItems(column).Skip(1);
        var types = items.Select(x => x?.GetType())
            .Distinct()
            .Where(x => x != null)
            .Where(x => x != typeof(DBNull))
            .ToArray();
        return types.Length == 1 ? types.First()! : typeof(object);
    }

    public string GetColumnHeader(int ci)
    {
        return GetRowItems(GetRow(0)).ElementAt(ci)?.ToString()??string.Empty;
    }
}
