namespace KustoLoco.Core.DataSource.Columns;

public interface INullableSet
{
    public bool IsNull(int i);
    public object ? NullableValue(int i);
    public int Length { get; }
}
