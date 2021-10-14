// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace BabyKusto.Core
{
    internal class EmptyTableSource : ITableSource
    {
        public TableSchema Schema => TableSchema.Empty;

        public IEnumerable<ITableChunk> GetData() => Enumerable.Empty<ITableChunk>();
    }
}
