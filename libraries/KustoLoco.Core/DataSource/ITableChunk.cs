// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace KustoLoco.Core.DataSource;

public interface ITableChunk
{
    ITableSource Table { get; }

    BaseColumn[] Columns { get; }

    int RowCount { get; }
}