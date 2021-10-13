using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public class ColumnDefinition
    {
        public ColumnDefinition(string columnName, KustoValueKind valueKind)
        {
            this.ColumnName = columnName;
            this.ValueKind = valueKind; 
        }

        public string ColumnName { get; }

        public KustoValueKind ValueKind { get; }
    }
}
