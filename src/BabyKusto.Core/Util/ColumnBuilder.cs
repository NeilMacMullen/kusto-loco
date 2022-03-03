// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Util
{
    public abstract class ColumnBuilder
    {
        public abstract void Add(object value);
        public abstract Column ToColumn();
    }

    public class ColumnBuilder<T> : ColumnBuilder
    {
        private readonly List<T> _data = new();

        public ColumnBuilder(TypeSymbol type)
        {
            Type = type;
        }

        public TypeSymbol Type { get; }

        public void Add(T value)
        {
            _data.Add(value);
        }
        public override void Add(object value)
        {
            _data.Add((T)value);
        }

        public override Column ToColumn()
        {
            return Column.Create(Type, _data.ToArray());
        }
    }
}
