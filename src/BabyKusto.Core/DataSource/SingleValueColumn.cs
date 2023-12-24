using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public class SingleValueColumn<T> : Column<T>
{
    private readonly int _rowCount;

    public SingleValueColumn(TypeSymbol type, T? value, int nominalRowCount)
        : base(type, new[] { value }) =>
        _rowCount = nominalRowCount;

    public override T? this[int index] => base[0];
    public override int RowCount => _rowCount;

    public int IndirectIndex(int index) => 0;

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < _rowCount; i++)
        {
            action(this[i]);
        }
    }

    public override BaseColumn Slice(int start, int length) => new SingleValueColumn<T>(Type, this[0], length);

    public override object? GetRawDataValue(int index) => this[IndirectIndex(index)];

    public Column<T> ResizeTo(int lookupsLength) => (Column<T>)Slice(0, lookupsLength);
}