//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Extensions;

namespace KustoLoco.Core.DataSource;

public class TableChunk : ITableChunk
{
    public static readonly TableChunk Empty = new(NullTableSource.Instance, []);

    public TableChunk(ITableSource table, BaseColumn[] columns)
    {
        ValidateTypes(table.Type, columns);
        ValidateRowCounts(columns);

        Table = table;
        Columns = columns;
    }

    public ITableSource Table { get; }

    public BaseColumn[] Columns { get; }

    public int RowCount => Columns.Length == 0 ? 0 : Columns[0].RowCount;

    private static void ValidateTypes(TableSymbol tableSymbol, BaseColumn[] columns)
    {
        if (columns.Length != tableSymbol.Columns.Count)
        {
            throw new ArgumentException(
                $"Invalid number of columns in chunk, got {columns.Length}, expected {tableSymbol.Columns.Count} per table schema.");
        }

        for (var i = 0; i < columns.Length; i++)
        {
            if (columns[i].Type.Simplify() != tableSymbol.Columns[i].Type.Simplify())
            {
                //TODO - nasty hack to fix up Kusto's odd integer rules.
                throw new ArgumentException(
                    $"Mismatched column[{i}] type in chunk, got {SchemaDisplay.GetText(columns[i].Type)}, expected {SchemaDisplay.GetText(tableSymbol.Columns[i].Type)}.");
            }
        }
    }

    private static void ValidateRowCounts(BaseColumn[] columns)
    {
        if (columns.Length == 0)
        {
            return;
        }

        var expected = columns[0].RowCount;
        for (var i = 1; i < columns.Length; i++)
        {
            if (columns[i].RowCount != expected)
            {
                throw new ArgumentException(
                    $"Mismatched column lengths, column[0] has {expected} rows, but column[{i}] has {columns[i].RowCount} rows.");
            }
        }
    }
}
