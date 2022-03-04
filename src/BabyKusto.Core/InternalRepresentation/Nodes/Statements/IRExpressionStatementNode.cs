// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRExpressionStatementNode : IRStatementNode
    {
        public IRExpressionStatementNode(IRNode expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public IRNode Expression { get; }
        
        public override int ChildCount => 1;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Expression,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitExpressionStatement(this, context);
        }

        public override string ToString()
        {
            return "ExpressionStatement";
        }
    }
}
