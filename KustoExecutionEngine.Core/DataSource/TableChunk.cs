using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public class TableChunk : ITableChunk
    {
        public TableChunk(TableSchema schema, Column[] columns)
        {
            this.Schema = schema;
            this.Columns = columns;
        }

        public TableSchema Schema { get; private set; }

        public Column[] Columns { get; private set; }

        public IRow GetRow(int index)
        {
            var values = new KeyValuePair<string, object?>[this.Schema.ColumnDefinitions.Count];
            for (int i = 0; i < this.Schema.ColumnDefinitions.Count; i++)
            {
                values[i] = new KeyValuePair<string, object?>(this.Schema.ColumnDefinitions[i].ColumnName, this.Columns[i][index]);
            }

            return new Row(values);
        }

        public void SetRow(IRow row, int index)
        {
            foreach (var kvp in row)
            {
                this.Columns[this.Schema.GetColumnIndex(kvp.Key)][index] = kvp.Value;
            }
        }
    }
}
