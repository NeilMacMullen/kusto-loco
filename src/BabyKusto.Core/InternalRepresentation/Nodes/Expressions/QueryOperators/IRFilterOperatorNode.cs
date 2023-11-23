// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRFilterOperatorNode : IRQueryOperatorNode
{
    public IRExpressionNode Condition;

    public IRFilterOperatorNode(IRExpressionNode condition, TypeSymbol resultType)
        : base(resultType) =>
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Condition,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        where TResult : class =>
        visitor.VisitFilterOperator(this, context);

    public override string ToString() => $"FilterOperator: {SchemaDisplay.GetText(ResultType)}";
}