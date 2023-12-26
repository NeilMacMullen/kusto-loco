namespace BabyKusto.Core;

public static class ColumnFactory
{
    public static TypedBaseColumn<T> Create<T>(T[] data) => new InMemoryColumn<T>(data);
}