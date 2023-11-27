// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Nodes;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core;

public enum IndirectPolicy
{
    Passthru,
    Map,
    SingleValue
}

public abstract class Column
{
    protected Column(TypeSymbol type) => Type = type ?? throw new ArgumentNullException(nameof(type));

    public TypeSymbol Type { get; }
    public abstract int RowCount { get; }

    public abstract object? GetRawDataValue(int index);

    public abstract Column Slice(int start, int end);
    public abstract void ForEach(Action<object?> action);
    public abstract ColumnBuilder CreateBuilder();
    public abstract IndirectColumnBuilder CreateIndirectBuilder(IndirectPolicy policy);

    public static Column<T> Create<T>(TypeSymbol type, T[] data) => new(type, data);
}

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

    public override Column Slice(int start, int length) => new SingleValueColumn<T>(Type, this[0], length);

    public override object? GetRawDataValue(int index) => this[IndirectIndex(index)];


    public override IndirectColumnBuilder CreateIndirectBuilder(IndirectPolicy policy) =>
        new IndirectColumnBuilder<T>(this, IndirectPolicy.SingleValue);
}

public class IndirectColumn<T> : Column<T>
{
    private readonly int[] _lookups;
    public readonly Column<T> BackingColumn;

    public IndirectColumn(int[] lookups, Column<T> backing)
        : base(backing.Type, Array.Empty<T>())
    {
        _lookups = lookups;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];
    public override int RowCount => _lookups.Length;

    public int IndirectIndex(int index) => _lookups[index];

    public override void ForEach(Action<object?> action)
    {
        foreach (var i in _lookups)
        {
            action(this[i]);
        }
    }

    public override Column Slice(int start, int length)
    {
        var slicedData = new int[length];
        Array.Copy(_lookups, start, slicedData, 0, length);
        return new IndirectColumn<T>(slicedData, BackingColumn);
    }


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));

    //TODO - this could be optimised to flatten the redirection chain
    public override IndirectColumnBuilder CreateIndirectBuilder(IndirectPolicy policy) =>
        new IndirectColumnBuilder<T>(this, policy, _lookups);
}

public class Column<T> : Column
{
    private readonly T?[] _data;

    public Column(TypeSymbol type, T?[] data)
        : base(type)
    {
        ValidateTypes(type, typeof(T));
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }


    public override int RowCount => _data.Length;

    public virtual T? this[int index] => _data[index];


    public override object? GetRawDataValue(int index) => _data.GetValue(index);

    public static void ValidateTypes(TypeSymbol typeSymbol, Type type)
    {
        var valid = false;
        if (typeSymbol == ScalarTypes.Int)
        {
            valid = type == typeof(int?);
        }
        else if (typeSymbol == ScalarTypes.Long)
        {
            valid = type == typeof(long?);
        }
        else if (typeSymbol == ScalarTypes.Real)
        {
            valid = type == typeof(double?);
        }
        else if (typeSymbol == ScalarTypes.Bool)
        {
            valid = type == typeof(bool?);
        }
        else if (typeSymbol == ScalarTypes.String)
        {
            valid = type == typeof(string);
        }
        else if (typeSymbol == ScalarTypes.DateTime)
        {
            valid = type == typeof(DateTime?);
        }
        else if (typeSymbol == ScalarTypes.TimeSpan)
        {
            valid = type == typeof(TimeSpan?);
        }
        else if (typeSymbol == ScalarTypes.Dynamic)
        {
            valid = type == typeof(JsonNode);
        }

        if (!valid)
        {
            throw new InvalidOperationException(
                $"Invalid column type {TypeNameHelper.GetTypeDisplayName(type)} for type symbol {SchemaDisplay.GetText(typeSymbol)}. Types must be nullable.");
        }
    }

    public override Column Slice(int start, int length)
    {
        var slicedData = new T[length];
        Array.Copy(_data, start, slicedData, 0, length);
        return Create(Type, slicedData);
    }

    public override void ForEach(Action<object?> action)
    {
        foreach (var item in _data)
        {
            action(item);
        }
    }

    public override ColumnBuilder CreateBuilder() => new ColumnBuilder<T>(Type);

    public override IndirectColumnBuilder CreateIndirectBuilder(IndirectPolicy policy) =>
        new IndirectColumnBuilder<T>(this, policy);
}

public class NullColumn : Column
{
    public static readonly NullColumn Instance = new();

    private NullColumn() : base(ScalarTypes.Null)
    {
    }

    public override int RowCount { get; } = 0;
    public override object? GetRawDataValue(int index) => throw new InvalidOperationException();

    public override Column Slice(int start, int end) => throw new InvalidOperationException();


    public override void ForEach(Action<object?> action)
    {
        throw new InvalidOperationException();
    }

    public override ColumnBuilder CreateBuilder() => throw new InvalidOperationException();


    public override IndirectColumnBuilder CreateIndirectBuilder(IndirectPolicy policy) =>
        throw new InvalidOperationException();
}