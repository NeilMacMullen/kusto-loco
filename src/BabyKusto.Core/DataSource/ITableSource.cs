using System.Collections.Generic;

namespace BabyKusto.Core
{
    public interface ITableSource
    {
        TableSchema Schema { get; }

        IEnumerable<ITableChunk> GetData();
    }
}
