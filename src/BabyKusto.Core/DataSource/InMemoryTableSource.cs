// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

/// <summary>
///     Use primary for EvaluationResults when all data has been materialized and is available
/// </summary>
public class InMemoryTableSource : ITableSource
{
    //actually only ever one chunk
    private readonly ITableChunk[] _data;

    public InMemoryTableSource(TableSymbol type, BaseColumn[] columns)
    {
        Type = type;
        _data = new ITableChunk[] { new TableChunk(this, columns) };
    }

    public int RowCount => _data.First().RowCount;
    public TableSymbol Type { get; }

    public IEnumerable<ITableChunk> GetData() => _data;

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
        => _data.ToAsyncEnumerable();

    //TODO - this is brittle and will break with empty tables
    public InMemoryTableSource ShareAs(string newName)
    {
        var ts = new TableSymbol(newName, Type.Columns);
        return new InMemoryTableSource(ts, _data.Single().Columns);
    }

    public static InMemoryTableSource FromITableSource(ITableSource other)
    {
        var chunk = ChunkHelpers.Reassemble(other.GetData().ToArray());

        return new InMemoryTableSource(other.Type,
                                       chunk.Columns);
    }

    public IEnumerable<object?> GetColumnData(int n)
    {
        var col =
            _data[0].Columns[n];
        var len = col.RowCount;

        return
            Enumerable.Range(0, len)
                      .Select(i => col.GetRawDataValue(i));
    }
}
