// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRPipeExpressionNode : IRExpressionNode
    {
        public IRPipeExpressionNode(IRExpressionNode expression, IRQueryOperatorNode @operator, TypeSymbol resultType)
            : base(resultType, EvaluatedExpressionKind.Table)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
        }

        public IRExpressionNode Expression { get; }
        public IRQueryOperatorNode Operator { get; }

        public override int ChildCount => 2;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Expression,
                1 => Operator,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitPipeExpression(this, context);
        }

        public override string ToString()
        {
            return $"PipeExpression: {ResultType.Display}";
        }
    }
}
