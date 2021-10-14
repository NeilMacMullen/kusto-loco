using System.Collections.Generic;
using System.Linq;

namespace KustoExecutionEngine.Core
{
    internal class EmptyTabularSourceV2 : ITabularSourceV2
    {
        public TableSchema Schema => TableSchema.Empty;

        public IEnumerable<ITableChunk> GetData() => Enumerable.Empty<ITableChunk>();
    }
}
