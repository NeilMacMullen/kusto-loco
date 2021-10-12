namespace KustoExecutionEngine.Core
{
    internal class EmptyTabularSource : ITabularSource
    {
        public IRow GetNextRow()
        {
            throw new NotImplementedException();
        }
    }
}
