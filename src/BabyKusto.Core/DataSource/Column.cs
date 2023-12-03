// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Nodes;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core;

public abstract class Column
{
    protected Column(TypeSymbol type) => Type = type ?? throw new ArgumentNullException(nameof(type));

    public TypeSymbol Type { get; }
    public abstract int RowCount { get; }

    public abstract object? GetRawDataValue(int index);

    public abstract Column Slice(int start, int end);
    public abstract void ForEach(Action<object?> action);

    public static Column<T> Create<T>(TypeSymbol type, T[] data) => new(type, data);
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


    public override object? GetRawDataValue(int index) => _data[index];

    public static void ValidateTypes(TypeSymbol typeSymbol, Type type)
    {
#if DEBUG
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
#endif
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

}