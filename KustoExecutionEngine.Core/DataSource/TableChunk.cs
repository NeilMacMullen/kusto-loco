using System.Collections.Generic;

namespace KustoExecutionEngine.Core.DataSource
{
    public class TableChunk : ITableChunk
    {
        public TableChunk(TableSchema schema, Column[] columns)
        {
            Schema = schema;
            Columns = columns;
        }

        public TableSchema Schema { get; }

        public Column[] Columns { get; }

        public int RowCount => Columns.Length == 0 ? 0 : Columns[0].RowCount;

        public IRow GetRow(int index)
        {
            var values = new KeyValuePair<string, object?>[Schema.ColumnDefinitions.Count];
            for (int i = 0; i < Schema.ColumnDefinitions.Count; i++)
            {
                values[i] = new KeyValuePair<string, object?>(Schema.ColumnDefinitions[i].ColumnName, Columns[i][index]);
            }

            return new Row(values);
        }

        public void SetRow(IRow row, int index)
        {
            foreach (var kvp in row)
            {
                Columns[Schema.GetColumnIndex(kvp.Key)][index] = kvp.Value;
            }
        }
    }
}
