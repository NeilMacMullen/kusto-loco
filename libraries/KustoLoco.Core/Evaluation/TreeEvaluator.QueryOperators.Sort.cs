// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using IComparer = System.Collections.IComparer;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitSortOperator(IRSortOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != EvaluationResult.Null);
        var sortColumns = new (IRExpressionNode Expression, IComparer Comparer)[node.Expressions.ChildCount];
        for (var i = 0; i < node.Expressions.ChildCount; i++)
        {
            var orderedExpression = node.Expressions.GetTypedChild(i);
            sortColumns[i] = (orderedExpression.Expression,
                BuiltInComparers.GetComparer(orderedExpression.SortDirection, orderedExpression.NullsDirection,
                    orderedExpression.Expression.ResultType));
        }

        var result = new SortResultTable(this, context, context.Left.Value, sortColumns);
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }
}
