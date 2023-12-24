// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public class NullTableSource : ITableSource
{
    public static readonly NullTableSource Instance = new();
    public TableSymbol Type { get; } = TableSymbol.Empty;
    public IEnumerable<ITableChunk> GetData() => Array.Empty<ITableChunk>();

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
        => AsyncEnumerable.Empty<ITableChunk>();
}

public class InMemoryTableSource : ITableSource
{
    private readonly ITableChunk[] _data;

    public InMemoryTableSource(TableSymbol type, BaseColumn[] columns)
    {
        Type = type;
        _data = new ITableChunk[] { new TableChunk(this, columns) };
    }

    public TableSymbol Type { get; }

    public IEnumerable<ITableChunk> GetData() => _data;

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default) =>
        _data.ToAsyncEnumerable();
}