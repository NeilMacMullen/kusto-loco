﻿using System;

namespace KustoLoco.Core.DataSource.Columns;

public class LambdaColumn<T> : TypedBaseColumn<T>
{
    private readonly Func<int, T?> _dataFetcher;
    private readonly int _length;

    public LambdaColumn(Func<int, T?> dataFetcher, int rowCount)
    {
        _dataFetcher = dataFetcher;
        _length = rowCount;
    }


    public override T? this[int index] => _dataFetcher(index);

    public override int RowCount => _length;

    public override object? GetRawDataValue(int index) => this[index];

    public override BaseColumn Slice(int start, int length)
    {
        return ChunkColumn<T>.Create(start, length, this);
    }

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
            action(GetRawDataValue(i));
    }
}
