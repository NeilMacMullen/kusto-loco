using System.Collections.Generic;

namespace KustoExecutionEngine.Core
{
    public interface ITableSource
    {
        TableSchema Schema { get; }

        IEnumerable<ITableChunk> GetData();
    }
}
