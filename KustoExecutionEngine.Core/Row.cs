using System.Collections;
using System.Collections.Generic;
using KustoExecutionEngine.Core.DataSource;

namespace KustoExecutionEngine.Core
{
    public class Row : IRow
    {
        public Row(TableSchema schema, object?[] values)
        {
            Schema = schema;
            Values = values;
        }

        public TableSchema Schema { get; }
        public object?[] Values { get; }
    }
}
