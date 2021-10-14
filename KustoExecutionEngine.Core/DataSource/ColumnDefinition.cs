namespace KustoExecutionEngine.Core
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
