// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    public abstract class EvaluationResult
    {
        protected EvaluationResult(TypeSymbol type)
        {
            Type = type;
        }

        public TypeSymbol Type { get; }

        public bool IsScalar => this is ScalarResult;
        public bool IsColumnar => this is ColumnarResult;
        public bool IsTabular => this is TabularResult;

        public override string ToString()
        {
            return $"{this.GetType().Name}: {Type.Display}";
        }
    }

    public sealed class ScalarResult : EvaluationResult
    {
        public ScalarResult(TypeSymbol type, object? value)
            : base(type)
        {
            if (!type.IsScalar)
            {
                throw new InvalidOperationException($"Expected scalar type for {nameof(ScalarResult)} value, got '{type.Display}'.");
            }

            Value = value;
        }

        public object? Value { get; }
    }

    public sealed class ColumnarResult : EvaluationResult
    {
        public ColumnarResult(Column column)
            : base(column.Type)
        {
            Column = column;
        }

        public Column Column { get; }
    }

    public sealed class TabularResult : EvaluationResult
    {
        public TabularResult(ITableSource value)
            : base(value.Type)
        {
            Value = value;
        }

        public ITableSource Value { get; }
    }
}
