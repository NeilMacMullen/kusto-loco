// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core
{
    public class TableChunk : ITableChunk
    {
        public TableChunk(ITableSource table, Column[] columns)
        {
            if (table.Type.Members.Count != columns.Length)
            {
                throw new ArgumentException($"Expected schema and columns to have the same lengths, found {table.Type.Members.Count} and {columns.Length}.");
            }

            Table = table;
            Columns = columns;
        }

        public ITableSource Table { get; }

        public Column[] Columns { get; }

        public int RowCount => Columns.Length == 0 ? 0 : Columns[0].RowCount;
    }
}
