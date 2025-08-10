//
// Licensed under the MIT License.

using System.Diagnostics;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitRenderOperator(IRRenderOperatorNode node, EvaluationContext context)
    {
        MyDebug.Assert(context.Left != EvaluationResult.Null);
       
        return TabularResult.CreateWithVisualisation(
            context.Left.Value,
            new VisualizationState(node.ChartType,node.Items));
    }
}
