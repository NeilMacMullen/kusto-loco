// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using BabyKusto.Core.Evaluation.BuiltIns;
using BabyKusto.Core.InternalRepresentation;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitUnaryExpression(IRUnaryExpressionNode node, EvaluationContext context)
        {
            var impl = node.GetOrSetCache(
                () =>
                {
                    var argumentExpressions = new IRExpressionNode[] { node.Expression };
                    var impl = node.OverloadInfo.ScalarImpl;
                    var resultKind = node.ResultKind;

                    return BuiltInsHelper.GetImplementation(argumentExpressions, impl, resultKind);
                });

            var val = node.Expression.Accept(this, context);
            Debug.Assert(val != null);
            var arguments = new[] { val };
            return impl(arguments);
        }

        public override EvaluationResult? VisitBinaryExpression(IRBinaryExpressionNode node, EvaluationContext context)
        {
            var impl = node.GetOrSetCache(
                () =>
                {
                    var argumentExpressions = new IRExpressionNode[] { node.Left, node.Right };
                    var impl = node.OverloadInfo.ScalarImpl;
                    var resultKind = node.ResultKind;

                    return BuiltInsHelper.GetImplementation(argumentExpressions, impl, resultKind);
                });

            var leftVal = node.Left.Accept(this, context);
            var rightVal = node.Right.Accept(this, context);
            Debug.Assert(leftVal != null && rightVal != null);
            var arguments = new[] { leftVal, rightVal };
            return impl(arguments);
        }
    }
}
