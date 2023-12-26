// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

public class EvaluationResult
{
    public static readonly EvaluationResult Null = new(NullTypeSymbol.Instance);


    protected EvaluationResult(TypeSymbol type) => Type = type;

    public TypeSymbol Type { get; }

    public bool IsScalar => this is ScalarResult;
    public bool IsColumnar => this is ColumnarResult;
    public bool IsTabular => this is TabularResult;

    public virtual int RowCount => 0;

    public override string ToString() => $"{GetType().Name}: {SchemaDisplay.GetText(Type)}";
}