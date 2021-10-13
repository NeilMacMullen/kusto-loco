using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public class TableSchema
    {
        public TableSchema(List<ColumnDefinition> columnDefinitions)
        {
            this.ColumnDefinitions = columnDefinitions; ;
        }

        public List<ColumnDefinition> ColumnDefinitions { get; }

        public int GetColumnIndex(string columnName)
        {
            return this.ColumnDefinitions.FindIndex(c => c.ColumnName == columnName);
        }
    }
}
