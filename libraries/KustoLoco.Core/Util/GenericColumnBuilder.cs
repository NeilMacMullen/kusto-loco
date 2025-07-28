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

[KustoGeneric(Types = "value")]
public sealed class GenericColumnBuilder<T> : BaseColumnBuilder
where T:class
{
    private readonly NullableSetBuilder<T> _data = new NullableSetBuilder<T>(10000);

    public GenericColumnBuilder() : this(string.Empty)
    {
    }

    public GenericColumnBuilder(string name)
    {
        Name = name;
    }

    public override int RowCount => _data.Length;
 
    public void Add( T? value) => _data.Add(value);

    public override void Add(object? value)
    {
        _data.Add(value);
    }

    public override BaseColumn ToColumn()
    {
        if (_nullableSet is { } set)
            return   ColumnFactory.CreateFromDataSet(set);

        return ColumnFactory.CreateFromDataSet(_data.ToNullableSet());
    }

}

[KustoGeneric(Types = "reference")]
public sealed class GenericColumnBuilder_ref<T> : BaseColumnBuilder
    where T : class
{
    private readonly NullableSetBuilder<T> _data = new NullableSetBuilder<T>(10000);

    public GenericColumnBuilder_ref() : this(string.Empty)
    {
    }

    public GenericColumnBuilder_ref(string name)
    {
        Name = name;
    }

    public override int RowCount => _data.Length;

    public void Add(T? value) => _data.Add(value);

    public override void Add(object? value)
    {
        _data.Add(value);
    }

    public override BaseColumn ToColumn()
    {
        if (_nullableSet is { } set)
            return ColumnFactory.CreateFromDataSet(set);

        return ColumnFactory.CreateFromDataSet(_data.ToNullableSet());
    }

}

