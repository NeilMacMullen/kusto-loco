// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using CommunityToolkit.HighPerformance.Buffers;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using NotNullStrings;

namespace KustoLoco.Core.Util;

public class ColumnBuilder<T> : BaseColumnBuilder
{
    private readonly List<T?> _data = [];

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

    public override void TrimExcess() =>
        _data.TrimExcess();

    public override void Add(object? value)
    {
        //prevent null strings being added
        if (typeof(T) == typeof(string) && value is null)
            value = string.Empty;

        if (typeof(T) == typeof(JsonNode))
        {
            if (value is T jn)
                _data.Add(jn);
            else 
                _data.Add(default);
        }
        else if (
            typeof(T) == typeof(DateTime?) &&
            value is DateTime dt)
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
        if (typeof(T) == typeof(string) && _data.Count > 10000)
        {
            var pool = new StringPool(1000);
            var c = _data.Select(d => pool.GetOrAdd((d as string).NullToEmpty()))
                .ToArray();
            return ColumnFactory.Create(c);
        }

        return ColumnFactory.Create(_data.ToArray());
    }

    public override Array GetDataAsArray() => _data.ToArray();
}
