// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

public class NullTypeSymbol : TypeSymbol
{
    public static readonly TypeSymbol Instance = new NullTypeSymbol();

    private NullTypeSymbol() : base(string.Empty)
    {
    }
}

public class EvaluationResult
{
    public static readonly EvaluationResult Null = new(NullTypeSymbol.Instance);


    protected EvaluationResult(TypeSymbol type) => Type = type;

    public TypeSymbol Type { get; }

    public bool IsScalar => this is ScalarResult;
    public bool IsColumnar => this is ColumnarResult;
    public bool IsTabular => this is TabularResult;

    public override string ToString() => $"{GetType().Name}: {SchemaDisplay.GetText(Type)}";
}

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

public sealed class ColumnarResult : EvaluationResult
{
    public ColumnarResult(Column column)
        : base(column.Type) =>
        Column = column;

    public Column Column { get; }
}

public sealed class TabularResult : EvaluationResult
{
    public TabularResult(ITableSource value, VisualizationState? visualizationState)
        : base(value.Type)
    {
        Value = value;
        VisualizationState = visualizationState;
    }

    public ITableSource Value { get; }

    public VisualizationState? VisualizationState { get; }
}

public record VisualizationState(string ChartType, string? ChartKind)
{
}