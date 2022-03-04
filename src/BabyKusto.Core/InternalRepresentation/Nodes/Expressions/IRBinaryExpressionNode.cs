// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRBinaryExpressionNode : IRExpressionNode
    {
        public IRBinaryExpressionNode(Signature signature, ScalarOverloadInfo overloadInfo, IRExpressionNode left, IRExpressionNode right, TypeSymbol resultType)
            : base(resultType, GetResultKind(left.ResultKind, right.ResultKind))
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
            OverloadInfo = overloadInfo ?? throw new ArgumentNullException(nameof(overloadInfo));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public Signature Signature { get; }
        public ScalarOverloadInfo OverloadInfo { get; }
        public IRExpressionNode Left { get; }
        public IRExpressionNode Right { get; }

        public override int ChildCount => 2;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => Left,
                1 => Right,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitBinaryExpression(this, context);
        }

        public override string ToString()
        {
            return $"BinaryExpression({Signature.Display}): {ResultType.Display}";
        }
    }
}
