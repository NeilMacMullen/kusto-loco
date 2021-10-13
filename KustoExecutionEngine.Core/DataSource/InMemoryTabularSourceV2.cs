using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public class InMemoryTabularSourceV2 : ITabularSourceV2
    {
        private readonly List<ITableChunk> _data;

        public InMemoryTabularSourceV2(TableSchema schema, List<ITableChunk> tableChunks)
        {
            this.Schema = schema;
            this._data = tableChunks;
        }

        public TableSchema Schema { get; private set; }

        public IEnumerator<ITableChunk> GetEnumerator()
        {
            foreach (var chunk in this._data)
            {
                yield return chunk;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
