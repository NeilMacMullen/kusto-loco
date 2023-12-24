namespace BabyKusto.Core.Util;

public abstract class BaseColumnBuilder
{
    public abstract int RowCount { get; }
    public abstract object? this[int index] { get; }
    public abstract void Add(object? value);
    public abstract void AddRange(BaseColumnBuilder other);
    public abstract BaseColumn ToColumn();
}