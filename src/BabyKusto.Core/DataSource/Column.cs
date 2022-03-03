// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core
{
    public abstract class Column
    {
        public Column(TypeSymbol type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public TypeSymbol Type { get; }
        public abstract int RowCount { get; }
        public abstract Array RawData { get; }


        public abstract Column Slice(int start, int end);
        public abstract void ForEach(Action<object> action);
        internal abstract ColumnBuilder CreateBuilder();

        public static Column<T> Create<T>(TypeSymbol type, T[] data)
        {
            return new Column<T>(type, data);
        }
    }

    public class Column<T> : Column
    {
        private readonly T[] _data;

        public Column(TypeSymbol type, T[] data)
            : base(type)
        {
            ValidateTypes(type, typeof(T));
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        private static void ValidateTypes(TypeSymbol typeSymbol, Type type)
        {
            bool valid = false;
            if (typeSymbol == ScalarTypes.Int)
            {
                valid = type == typeof(int);
            }
            else if (typeSymbol == ScalarTypes.Long)
            {
                valid = type == typeof(long);
            }
            else if (typeSymbol == ScalarTypes.Real)
            {
                valid = type == typeof(double);
            }
            else if (typeSymbol == ScalarTypes.Bool)
            {
                valid = type == typeof(bool);
            }
            else if (typeSymbol == ScalarTypes.String)
            {
                valid = type == typeof(string);
            }
            else if (typeSymbol == ScalarTypes.DateTime)
            {
                valid = type == typeof(DateTime);
            }
            else if (typeSymbol == ScalarTypes.TimeSpan)
            {
                valid = type == typeof(TimeSpan);
            }

            if (!valid)
            {
                throw new InvalidOperationException($"Invalid column type {TypeNameHelper.GetTypeDisplayName(type)} for type symbol {typeSymbol.Display}.");
            }
        }

        public override int RowCount => _data.Length;
        public override Array RawData => _data;

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public Span<T> GetSpan(int start, int length)
        {
            return _data.AsSpan(start, length);
        }

        public override Column Slice(int start, int length)
        {
            return Column.Create(Type, GetSpan(start, length).ToArray());
        }

        public override void ForEach(Action<object> action)
        {
            foreach (var item in _data)
            {
                action(item);
            }
        }

        internal override ColumnBuilder CreateBuilder()
        {
            return new ColumnBuilder<T>(Type);
        }
    }
}
