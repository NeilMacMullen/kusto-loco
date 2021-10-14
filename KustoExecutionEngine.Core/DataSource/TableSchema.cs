using System.Collections.Generic;

namespace KustoExecutionEngine.Core.DataSource
{
    public class TableSchema
    {
        public static readonly TableSchema Empty = new TableSchema(new List<ColumnDefinition>());

        public TableSchema(List<ColumnDefinition> columnDefinitions)
        {
            this.ColumnDefinitions = columnDefinitions;
        }

        public List<ColumnDefinition> ColumnDefinitions { get; }

        public int GetColumnIndex(string columnName)
        {
            return this.ColumnDefinitions.FindIndex(c => c.ColumnName == columnName);
        }
    }
}
