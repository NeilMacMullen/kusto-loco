// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core;

public interface ITableChunk
{
    ITableSource Table { get; }

    Column[] Columns { get; }

    int RowCount { get; }
}