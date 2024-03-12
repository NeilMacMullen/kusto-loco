using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource;

public static class ColumnFactory
{
    public static TypedBaseColumn<T> Create<T>(T[] data) =>
        data.Length == 1
            ? new SingleValueColumn<T>(data[0], 1)
            : new InMemoryColumn<T>(data);

    public static BaseColumn Inflate(BaseColumn column, int logicalRowCount)
    {
        var value = column.GetRawDataValue(0);
        return ColumnHelpers.CreateFromScalar(value, column.Type, logicalRowCount);
    }
}