using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

public sealed class ScalarResult : EvaluationResult
{
    public ScalarResult(TypeSymbol type, object? value)
        : base(type)
    {
        if (!type.IsScalar)
        {
            throw new InvalidOperationException(
                $"Expected scalar type for {nameof(ScalarResult)} value, got '{SchemaDisplay.GetText(type)}'.");
        }

        Value = value;
    }

    public object? Value { get; }
}