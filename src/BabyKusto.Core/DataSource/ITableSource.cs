// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace BabyKusto.Core
{
    public interface ITableSource
    {
        TableSchema Schema { get; }

        IEnumerable<ITableChunk> GetData();
    }
}
