using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public interface ITabularSourceV2 : IEnumerable<ITableChunk>
    {
        TableSchema Schema { get; }
    }
}
