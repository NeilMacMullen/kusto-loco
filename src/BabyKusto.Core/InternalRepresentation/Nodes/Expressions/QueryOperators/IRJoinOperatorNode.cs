// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal enum IRJoinKind
    {
        InnerUnique,
        Inner,
        LeftOuter,
        RightOuter,
        FullOuter,
        LeftSemi,
        RightSemi,
        LeftAnti,
        RightAnti,
    }

    internal class IRJoinOperatorNode : IRQueryOperatorNode
    {
        public IRJoinOperatorNode(IRJoinKind kind, IRExpressionNode expression, IRListNode<IRExpressionNode> onExpressions, TypeSymbol resultType)
            : base(resultType)
        {
            Kind = kind;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            OnExpressions = onExpressions ?? throw new ArgumentNullException(nameof(onExpressions));
        }

        public IRJoinKind Kind { get; }
        public IRExpressionNode Expression { get; }
        public IRListNode<IRExpressionNode> OnExpressions { get; }

        public override int ChildCount => 2;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Expression,
                1 => OnExpressions,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitJoinOperator(this, context);
        }

        public override string ToString()
        {
            return $"JoinOperator(kind={Kind}): {ResultType.Display}";
        }
    }
}
