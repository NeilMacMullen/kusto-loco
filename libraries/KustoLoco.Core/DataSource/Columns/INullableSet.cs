using System;

namespace KustoLoco.Core.DataSource.Columns;

public interface INullableSet
{
    public bool IsNull(int i);
    public object ? NullableObject(int i);
    public int Length { get; }
    public Array GetDataAsArray();
    public bool NoNulls { get; }
    public Type UnderlyingType { get; }
}
