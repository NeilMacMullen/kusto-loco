// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitUnaryExpression(IRUnaryExpressionNode node, EvaluationContext context)
    {
        var impl = node.GetOrSetCache(
            () => BuiltInsHelper.GetScalarImplementation(node.OverloadInfo.ScalarImpl,
                node.ResultKind, node.ResultType, EvaluationHints.None));

        var val = node.Expression.Accept(this, context);
        Debug.Assert(val != null);
        var arguments = new[] { val };
        return impl(arguments);
    }

    public override EvaluationResult VisitBinaryExpression(IRBinaryExpressionNode node, EvaluationContext context)
    {
        var impl = node.GetOrSetCache(
            () => BuiltInsHelper.GetScalarImplementation(node.OverloadInfo.ScalarImpl,
                node.ResultKind, node.ResultType, EvaluationHints.None));
        //TODO - some kinds of expression (and/or) could be short-circuited but
        //this approach evaluates nodes from the bottom up before we can get
        //to the short-circuiting It's hard to see how to overcome this except by 
        //1) flagging and/or as possible short-circuit operations
        //2) checking here for the short circuit if one of the arms of the expression 
        //   is single value.  But of course we won't find that until we try to evaluate...
        var leftVal = node.Left.Accept(this, context);
        var rightVal = node.Right.Accept(this, context);
        Debug.Assert(leftVal != null && rightVal != null);
        var arguments = new[] { leftVal, rightVal };
        return impl(arguments);
    }
}