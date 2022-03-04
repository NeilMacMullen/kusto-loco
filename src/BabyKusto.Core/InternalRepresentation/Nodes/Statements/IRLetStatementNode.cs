// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRLetStatementNode : IRStatementNode
    {
        public IRLetStatementNode(Symbol symbol, IRExpressionNode expression)
        {
            Symbol = symbol;
            Expression = expression;
        }

        public Symbol Symbol { get; }
        public IRExpressionNode Expression { get; }

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
            return visitor.VisitLetStatement(this, context);
        }

        public override string ToString()
        {
            return $"LetStatement {{{Symbol.Name} = ...}}";
        }
    }
}
