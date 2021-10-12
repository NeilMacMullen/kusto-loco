namespace KustoExecutionEngine.Core
{
    public interface ITabularSource
    {
        IRow? GetNextRow();
    }

    public interface IRow
    {
    }
}
