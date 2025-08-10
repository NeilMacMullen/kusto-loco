﻿//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRSummarizeOperatorNode : IRQueryOperatorNode
{
    public IRSummarizeOperatorNode(IRListNode<IRExpressionNode> aggregations,
        IRListNode<IRExpressionNode> byColumns, TypeSymbol resultType)
        : base(resultType)
    {
        Aggregations = aggregations ?? throw new ArgumentNullException(nameof(aggregations));
        ByColumns = byColumns ?? throw new ArgumentNullException(nameof(byColumns));
    }

    public IRListNode<IRExpressionNode> Aggregations { get; }
    public IRListNode<IRExpressionNode> ByColumns { get; }


    public override int ChildCount => 2;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Aggregations,
            1 => ByColumns,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitSummarizeOperator(this, context);

    public override string ToString() => $"SummarizeOperator: {SchemaDisplay.GetText(ResultType)}";
}
