using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public interface ITableChunk
    {
        TableSchema Schema { get; }

        Column[] Columns { get; }
    }
}
