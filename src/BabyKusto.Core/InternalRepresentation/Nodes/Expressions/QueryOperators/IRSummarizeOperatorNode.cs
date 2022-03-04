// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRSummarizeOperatorNode : IRQueryOperatorNode
    {
        public IRSummarizeOperatorNode(IRListNode<IRExpressionNode> aggregations, IRListNode<IRExpressionNode> byColumns, TypeSymbol resultType)
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

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitSummarizeOperator(this, context);
        }

        public override string ToString()
        {
            return $"SummarizeOperator: {ResultType.Display}";
        }
    }
}
