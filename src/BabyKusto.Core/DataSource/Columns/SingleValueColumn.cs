using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public class SingleValueColumn<T> : TypedBaseColumn<T>
{
    private readonly int _rowCount;

    private readonly T? Value;

    public SingleValueColumn(TypeSymbol type, T? value, int nominalRowCount)
        : base(type)
    {
        _rowCount = nominalRowCount;
        Value = value;
    }

    public override T? this[int index] => Value;
    public override int RowCount => _rowCount;

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < _rowCount; i++)
        {
            action(Value);
        }
    }

    public override BaseColumn Slice(int start, int length) => new SingleValueColumn<T>(Type, Value, length);

    public override object? GetRawDataValue(int index) => Value;

    public TypedBaseColumn<T> ResizeTo(int lookupsLength) => new SingleValueColumn<T>(Type, Value, lookupsLength);
}