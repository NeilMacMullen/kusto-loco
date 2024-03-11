// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;


namespace BabyKusto.Core.Util;

public class ColumnBuilder<T> : BaseColumnBuilder
{
    private readonly List<T?> _data = new();

    public override int RowCount => _data.Count;
    public override object? this[int index] => _data[index];

    public void Add(T value)
    {
        _data.Add(value);
    }

    public override void AddRange(BaseColumnBuilder other)
    {
        if (other is not ColumnBuilder<T> typedOther)
        {
            throw new ArgumentException(
                $"Expected other of type {TypeNameHelper.GetTypeDisplayName(typeof(ColumnBuilder<T>))}, found {TypeNameHelper.GetTypeDisplayName(other)}");
        }

        _data.AddRange(typedOther._data);
    }

    public override void Add(object? value)
    {
        if (typeof(T) == typeof(JsonNode))
            _data.Add((T?)value);
        else
            _data.Add(TypeMapping.CastOrConvertToNullable<T>(value));
    }

    public override BaseColumn ToColumn() => ColumnFactory.Create(_data.ToArray());
    public override Array GetDataAsArray() => _data.ToArray();
}