﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Internal;

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
        _data.Add((T?)value);
    }

    public override BaseColumn ToColumn() => ColumnFactory.Create(
        TypeMapping.SymbolForType(typeof(T)),
        _data.ToArray());
}