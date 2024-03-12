// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Kusto.Language.Symbols;

namespace KustoLoco.Core;

public interface ITableSource
{
    TableSymbol Type { get; }
    public string Name => Type.Name;

    IEnumerable<ITableChunk> GetData();
    IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default);
}