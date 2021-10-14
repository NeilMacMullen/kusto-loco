// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core
{
    public interface ITableChunk
    {
        TableSchema Schema { get; }

        Column[] Columns { get; }

        int RowCount { get; }

        IRow GetRow(int index);
    }
}
