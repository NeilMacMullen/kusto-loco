// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public class InMemoryTableSource : ITableSource
{
    private readonly ITableChunk[] _data;

    public InMemoryTableSource(TableSymbol type, Column[] columns)
    {
        Type = type;
        _data = new ITableChunk[] { new TableChunk(this, columns) };
    }

    public TableSymbol Type { get; }

    public IEnumerable<ITableChunk> GetData() => _data;

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default) =>
        _data.ToAsyncEnumerable();
}