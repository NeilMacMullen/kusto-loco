using System.Linq;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource;

public static class ColumnFactory
{
    public static TypedBaseColumn<T> CreateFromObjects<T>(object?[] data) =>
        data.Length == 1
            ? new SingleValueColumn<T>(data[0], 1)
            : new InMemoryColumn<T>(data);

    public static TypedBaseColumn<T> Create<T>(T?[] data) =>
        CreateFromObjects<T>(data.Cast<object?>().ToArray());

    public static BaseColumn Inflate(BaseColumn column, int logicalRowCount)
    {
        var value = column.GetRawDataValue(0);
        return ColumnHelpers.CreateFromScalar(value, column.Type, logicalRowCount);
    }
}
