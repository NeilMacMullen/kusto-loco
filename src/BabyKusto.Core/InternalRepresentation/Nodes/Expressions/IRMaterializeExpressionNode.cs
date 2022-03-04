// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRMaterializeExpressionNode : IRExpressionNode
    {
        private readonly IRExpressionNode _expression;

        public IRMaterializeExpressionNode(IRExpressionNode expression, TypeSymbol resultType)
            : base(resultType, expression.ResultKind)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override int ChildCount => 1;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => _expression,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitMaterializeExpression(this, context);
        }

        public override string ToString()
        {
            return "MaterializeExpression";
        }
    }
}
