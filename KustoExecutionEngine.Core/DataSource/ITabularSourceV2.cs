using System.Collections.Generic;

namespace KustoExecutionEngine.Core
{
    public interface ITabularSourceV2
    {
        TableSchema Schema { get; }

        IEnumerable<ITableChunk> GetData();
    }
}
