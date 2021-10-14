namespace BabyKusto.Core
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
