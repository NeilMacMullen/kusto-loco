using System;

namespace KustoLoco.Core.DataSource.Columns;

public interface INullableSet
{
    public bool IsNull(int i);
    public object ? NullableValue(int i);
    public int Length { get; }
    public Array GetDataAsArray(bool allowNonNullReturn);
    public bool NoNulls { get; }
    public Type UnderlyingType { get; }
}
