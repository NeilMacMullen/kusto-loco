// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitRenderOperator(IRRenderOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != EvaluationResult.Null);
       
        return TabularResult.CreateWithVisualisation(
            context.Left.Value,
            new VisualizationState(ChartType: node.ChartType,node.Items));
    }
}
