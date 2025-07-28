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

public sealed class OldColumnBuilder<T> : BaseColumnBuilder
{
    private readonly List<object?> _data = [];

    public OldColumnBuilder() : this(string.Empty)
    {
    }

    public OldColumnBuilder(string name)
    {
        Name = name;
    }

    public override int RowCount => _data.Count;
   
    public void Add(T value) => _data.Add(value);

    public override void Add(object? value)
    {
        //prevent null strings being added
        if (typeof(T) == typeof(string) && value is null)
            value = string.Empty;

        if (typeof(T) == typeof(JsonNode))
        {
          //  if (value is T jn)
                _data.Add(value);
           // else 
           //     _data.Add(default);
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
        if (_nullableSet is { } set)
            return ColumnFactory.CreateFromDataSet<T>(set);
        
        if (typeof(T) == typeof(string) && _data.Count > 10000)
        {
            var pool = new StringPool(1000);
            var c = _data.Select(d => pool.GetOrAdd((d as string).NullToEmpty()))
                .ToArray();
            return ColumnFactory.Create(c);
        }

        return ColumnFactory.CreateFromObjects<T>(_data.ToArray());
    }

}
