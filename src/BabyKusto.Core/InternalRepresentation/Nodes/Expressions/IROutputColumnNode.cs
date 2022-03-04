// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IROutputColumnNode : IRExpressionNode
    {
        public IROutputColumnNode(ColumnSymbol symbol, IRExpressionNode expression)
            : base(symbol.Type, expression.ResultKind)
        {
            this.Symbol = symbol;
            this.Expression = expression;
        }

        public ColumnSymbol Symbol { get; }
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
            return visitor.VisitOutputColumn(this, context);
        }

        public override string ToString()
        {
            return $"OutputColumn({Symbol.Name}: {ResultType.Display})";
        }
    }
}
