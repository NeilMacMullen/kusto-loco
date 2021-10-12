namespace KustoExecutionEngine.Core
{
    internal class DerivedTabularSource : ITabularSource
    {
        private readonly ITabularSource _source;
        private readonly Func<IRow, IRow?> _mapFn;

        public DerivedTabularSource(ITabularSource source, Func<IRow, IRow?> mapFn)
        {
            _source = source;
            _mapFn = mapFn;
        }

        public IRow? GetNextRow()
        {
            IRow? row;
            while ((row = _source.GetNextRow()) != null)
            {
                var mapped = _mapFn(row);
                if (mapped != null)
                {
                    return mapped;
                }
            }

            return null;
        }
    }
}
