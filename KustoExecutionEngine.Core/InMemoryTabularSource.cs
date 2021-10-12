namespace KustoExecutionEngine.Core
{
    internal class InMemoryTabularSource : ITabularSource
    {
        private readonly List<IRow> _data;
        private int _index;

        public InMemoryTabularSource(params IRow[] data)
        {
            _data = new List<IRow>(data);
        }

        public IRow? GetNextRow()
        {
            if (_index < _data.Count)
            {
                return _data[_index++];
            }

            return null;
        }
    }
}
