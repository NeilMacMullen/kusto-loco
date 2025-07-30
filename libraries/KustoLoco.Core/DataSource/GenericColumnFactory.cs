using KustoLoco.Core.DataSource.Columns;
using System.Linq;

namespace KustoLoco.Core.DataSource;

[KustoGeneric(Types = "all")]
public static class GenericColumnFactory<T>
{

    public static GenericTypedBaseColumn<T> CreateFromObject(object? data, int rowcount)
        => new GenericSingleValueColumn<T>(data, 1);

    public static GenericTypedBaseColumn<T> CreateFromObjects(object?[] data)
        =>
            data.Length == 1
                ? new GenericSingleValueColumn<T>(data[0], 1)
                : new GenericInMemoryColumn<T>(data);

    public static GenericTypedBaseColumn<T> CreateFromDataSet(NullableSet<T> data)
        =>
            data.Length == 1
                ? new GenericSingleValueColumn<T>(data.NullableValue(0), 1)
                : new GenericInMemoryColumn<T>(data);

    public static GenericTypedBaseColumn<T> Create(T?[] data)
        =>
            CreateFromObjects(data.Cast<object?>().ToArray());


}
