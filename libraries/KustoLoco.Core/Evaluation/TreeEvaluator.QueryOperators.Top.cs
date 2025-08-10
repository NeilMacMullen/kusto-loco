//
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using IComparer = System.Collections.IComparer;

namespace KustoLoco.Core.Evaluation;
internal partial class TreeEvaluator
{
    public override EvaluationResult VisitTopOperator(IRTopOperatorNode node, EvaluationContext context)
    {
        MyDebug.Assert(context.Left != EvaluationResult.Null);
        var sortColumns = new (IRExpressionNode Expression, IComparer Comparer)[1];

        var orderedExpression = node.ByExpression;
        sortColumns[0] = (orderedExpression.Expression,
            BuiltInComparers.GetComparer(orderedExpression.SortDirection, orderedExpression.NullsDirection,
                orderedExpression.Expression.ResultType));
        var sortResult = new SortResultTable(this, context, context.Left.Value, sortColumns);

        var countExpressionResult = node.CountExpression.Accept(this, context);
        MyDebug.Assert(countExpressionResult != EvaluationResult.Null);
        var count = (ScalarResult)countExpressionResult;
        var result =
            new TakeResultTable(sortResult, count.Value == null ? 0 : Convert.ToInt32(count.Value));
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }
}
