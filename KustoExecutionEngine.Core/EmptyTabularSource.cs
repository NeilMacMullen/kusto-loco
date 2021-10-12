namespace KustoExecutionEngine.Core
{
    internal class EmptyTabularSource : ITabularSource
    {
        public IRow? GetNextRow()
        {
            return null;
        }
    }
}
