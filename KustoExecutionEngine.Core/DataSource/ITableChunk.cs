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

        IRow GetRow(int index);

        void SetRow(IRow row, int index);
    }
}
