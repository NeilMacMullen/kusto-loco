using System.Collections.Generic;
using System.Linq;

namespace KustoExecutionEngine.Core
{
    internal class EmptyTableSource : ITableSource
    {
        public TableSchema Schema => TableSchema.Empty;

        public IEnumerable<ITableChunk> GetData() => Enumerable.Empty<ITableChunk>();
    }
}
