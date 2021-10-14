using System.Collections.Generic;

namespace KustoExecutionEngine.Core.DataSource
{
    public class InMemoryTabularSourceV2 : ITabularSourceV2
    {
        private readonly ITableChunk[] _data;

        public InMemoryTabularSourceV2(TableSchema schema, Column[] columns)
        {
            this.Schema = schema;
            this._data = new ITableChunk[] { new TableChunk(Schema, columns) };
        }

        public TableSchema Schema { get; private set; }

        public IEnumerable<ITableChunk> GetData() => _data;
    }
}
