// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core.Extensions;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public class TableChunk : ITableChunk
{
    public static readonly TableChunk Empty = new(NullTableSource.Instance, Array.Empty<Column>());

    public TableChunk(ITableSource table, Column[] columns)
    {
        ValidateTypes(table.Type, columns);
        ValidateRowCounts(columns);

        Table = table;
        Columns = columns;
    }

    public ITableSource Table { get; }

    public Column[] Columns { get; }

    public int RowCount => Columns.Length == 0 ? 0 : Columns[0].RowCount;

    private static void ValidateTypes(TableSymbol tableSymbol, Column[] columns)
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
                throw new ArgumentException(
                    $"Mismatched column[{i}] type in chunk, got {SchemaDisplay.GetText(columns[i].Type)}, expected {SchemaDisplay.GetText(tableSymbol.Columns[i].Type)}.");
            }
        }
    }

    private static void ValidateRowCounts(Column[] columns)
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
                    $"Mismatched column lengths, column[0] has {expected} rows, but column[{i} has {columns[i].RowCount} rows.");
            }
        }
    }
}