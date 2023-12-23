// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRToScalarExpressionNode : IRExpressionNode
{
    // Irrespective of the result kind of the expression, the result is always a Scalar
    public IRToScalarExpressionNode(IRExpressionNode expression, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Scalar)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }


    public IRExpressionNode Expression { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Expression,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitToScalarExpressionNode(this, context);

    public override string ToString() => $"ToScalarExpression: {SchemaDisplay.GetText(ResultType)}";
}