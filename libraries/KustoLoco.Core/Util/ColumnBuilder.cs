// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using CommunityToolkit.HighPerformance.Buffers;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Util;

public class ColumnBuilder<T> : BaseColumnBuilder
{
    private readonly List<T?> _data = [];
    private StringPool _pool=new StringPool(1000);
    public ColumnBuilder() : this(string.Empty)
    {
    }

    public ColumnBuilder(string name)
    {
        Name = name;
        
    }

    public override int RowCount => _data.Count;
    public override object? this[int index] => _data[index];

    public void Add(T value) => _data.Add(value);

    public override void AddRange(BaseColumnBuilder other)
    {
        if (other is not ColumnBuilder<T> typedOther)
            throw new ArgumentException(
                $"Expected other of type {TypeNameHelper.GetTypeDisplayName(typeof(ColumnBuilder<T>))}, found {TypeNameHelper.GetTypeDisplayName(other)}");

        _data.AddRange(typedOther._data);
    }

    public override void AddCapacity(int n) =>
        _data.Capacity = _data.Count + n;

    public override void TrimExcess()
    {
        _pool.Reset();
        _data.TrimExcess();
    }

    public override void Add(object? value)
    {
        if (typeof(T) == typeof(JsonNode))
        {
            _data.Add((T?)value);
        }
        else if (value is string str)
        {
            str = _pool.GetOrAdd(str);
            _data.Add(TypeMapping.CastOrConvertToNullable<T>(str));
        }
        else if (value is DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            if (dt.Kind == DateTimeKind.Local) dt = dt.ToUniversalTime();
            _data.Add(TypeMapping.CastOrConvertToNullable<T>(dt));
        }
        else
        {
            _data.Add(TypeMapping.CastOrConvertToNullable<T>(value));
        }
    }

    public override BaseColumn ToColumn()
    {
        TrimExcess();
        return ColumnFactory.Create(_data.ToArray());
    }

    public override Array GetDataAsArray() => _data.ToArray();
}
