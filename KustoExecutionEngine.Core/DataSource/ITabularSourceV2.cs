using System.Collections.Generic;

namespace KustoExecutionEngine.Core.DataSource
{
    public interface ITabularSourceV2
    {
        TableSchema Schema { get; }

        IEnumerable<ITableChunk> GetData();
    }
}
