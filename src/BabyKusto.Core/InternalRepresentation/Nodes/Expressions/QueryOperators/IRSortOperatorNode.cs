// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRSortOperatorNode : IRQueryOperatorNode
    {
        public IRSortOperatorNode(IRListNode<IROrderedExpressionNode> expressions, TypeSymbol resultType)
            : base(resultType)
        {
            this.Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
        }

        public IRListNode<IROrderedExpressionNode> Expressions { get; }

        public override int ChildCount => 1;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Expressions,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitSortOperator(this, context);
        }

        public override string ToString()
        {
            return $"SortOperator: {ResultType.Display}";
        }
    }
}
