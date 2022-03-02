// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Kusto.Language.Symbols;

namespace BabyKusto.Core
{
    public interface ITableSource
    {
        TableSymbol Type { get; }

        IEnumerable<ITableChunk> GetData();
        IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default);
    }
}
