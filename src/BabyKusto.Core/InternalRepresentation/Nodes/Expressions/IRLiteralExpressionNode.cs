// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRLiteralExpressionNode : IRExpressionNode
{
    public IRLiteralExpressionNode(object? value, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Scalar) =>
        Value = value;

    public object? Value { get; }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitLiteralExpression(this, context);

    public override string ToString() => $"LiteralExpression: {SchemaDisplay.GetText(ResultType)} = {Value}";
}