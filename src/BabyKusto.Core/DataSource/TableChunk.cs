// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core
{
    public class TableChunk : ITableChunk
    {
        public TableChunk(ITableSource table, Column[] columns)
        {
            ValidateTypes(table.Type, columns);

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
                throw new ArgumentException($"Invalid number of columns in chunk, got {columns.Length}, expected {tableSymbol.Columns.Count} per table schema.");
            }

            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].Type != tableSymbol.Columns[i].Type)
                {
                    throw new ArgumentException($"Mismatched column[{i}] type in chunk, got {columns[i].Type.Display}, expected {tableSymbol.Columns[i].Type.Display}.");
                }
            }
        }
    }
}
