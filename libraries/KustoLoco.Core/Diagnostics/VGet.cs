using System.Collections.Generic;
using System.Collections.Immutable;
using Kusto.Language;
using Kusto.Language.Syntax;
using KustoLoco.Core.Evaluation;
using NotNullStrings;

namespace KustoLoco.Core.Diagnostics;

internal class VGet
{
    public VisualizationState State = VisualizationState.Empty;

    public void Walk(KustoCode code)
    {
        SyntaxElement.WalkNodes(
            code.Syntax,
            node =>
            {
                if (node is RenderOperator render)
                {
                    State = HandleRenderOperator(render);
                }
            },
            node => { });
    }

    private VisualizationState HandleRenderOperator(RenderOperator node)
    {
        var chartType = node.ChartType.Text;
        var items = new Dictionary<string, string>();
        if (node.WithClause == null)
            return new VisualizationState(chartType,
                ImmutableDictionary<string, string>.Empty);

        foreach (var prop in node.WithClause.Properties)
        {
            var element = prop.Element;

            if (element.Expression is LiteralExpression literalExpression)
            {
                var val = literalExpression.LiteralValue;
                items.Add(element.Name.SimpleName, val.ToString().NullToEmpty());
            }
        }

        return new VisualizationState(chartType,
            items.ToImmutableDictionary());
    }
}
