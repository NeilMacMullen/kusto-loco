using System.Collections;

namespace KustoExecutionEngine.Core.DataSource
{
    internal class EmptyTabularSourceV2 : ITabularSourceV2
    {
        public TableSchema Schema => throw new NotImplementedException();

        public IEnumerator<ITableChunk> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
