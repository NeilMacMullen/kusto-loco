// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRUnaryExpressionNode : IRExpressionNode
    {
        public IRUnaryExpressionNode(Signature signature, ScalarOverloadInfo overloadInfo, IRExpressionNode expression, TypeSymbol resultType)
            : base(resultType, expression.ResultKind)
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
            OverloadInfo = overloadInfo ?? throw new ArgumentNullException(nameof(overloadInfo));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Signature Signature { get; }
        public ScalarOverloadInfo OverloadInfo { get; }
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
            return visitor.VisitUnaryExpression(this, context);
        }

        public override string ToString()
        {
            return $"UnaryExpression({Signature.Display}): {ResultType.Display}";
        }
    }
}
