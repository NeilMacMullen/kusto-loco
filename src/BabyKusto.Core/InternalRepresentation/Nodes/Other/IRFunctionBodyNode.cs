// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRFunctionBodyNode : IRNode
    {
        public IRFunctionBodyNode(IRListNode<IRStatementNode> statements, IRExpressionNode expression)
        {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public IRListNode<IRStatementNode> Statements { get; }
        public IRExpressionNode Expression { get; }

        public override int ChildCount => 2;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Statements,
                1 => Expression,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitFunctionBody(this, context);
        }

        public override string ToString()
        {
            return $"FunctionBody: {Expression.ResultType.Display}";
        }
    }
}
