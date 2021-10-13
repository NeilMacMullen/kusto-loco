using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    internal class TableChunk : ITableChunk
    {
        public TableChunk(TableSchema schema, Column[] columns)
        {
            this.Schema = schema;
            this.Columns = columns;
        }

        public TableSchema Schema { get; private set; }

        public Column[] Columns { get; private set; }
    }
}
