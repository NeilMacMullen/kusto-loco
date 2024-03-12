// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using KustoLoco.Core.InternalRepresentation;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitRenderOperator(IRRenderOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != EvaluationResult.Null);

        return TabularResult.CreateWithVisualisation(
            context.Left.Value,
            new VisualizationState(ChartType: node.ChartType, node.Items));
    }
}