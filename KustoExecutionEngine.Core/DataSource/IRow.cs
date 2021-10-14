namespace KustoExecutionEngine.Core
{
    public interface IRow
    {
        TableSchema Schema { get; }

        object?[] Values { get; }
    }
}
