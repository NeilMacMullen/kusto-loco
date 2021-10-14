namespace BabyKusto.Core
{
    public interface IRow
    {
        TableSchema Schema { get; }

        object?[] Values { get; }
    }
}
