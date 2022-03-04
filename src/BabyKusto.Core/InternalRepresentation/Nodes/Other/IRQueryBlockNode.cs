// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRQueryBlockNode : IRNode
    {
        public IRQueryBlockNode(IRListNode<IRStatementNode> statements)
        {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }

        public IRListNode<IRStatementNode> Statements { get; }

        public override int ChildCount => 1;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Statements,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitQueryBlock(this, context);
        }

        public override string ToString()
        {
            return "QueryBlock";
        }
    }
}
