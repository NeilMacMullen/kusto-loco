﻿//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

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
         =>
        visitor.VisitFilterOperator(this, context);

    public override string ToString() => $"FilterOperator: {SchemaDisplay.GetText(ResultType)}";
}