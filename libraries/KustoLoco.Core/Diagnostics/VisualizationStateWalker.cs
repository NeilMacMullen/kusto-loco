using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Kusto.Language;
using Kusto.Language.Syntax;
using KustoLoco.Core.Evaluation;
using NotNullStrings;

namespace KustoLoco.Core.Diagnostics;

/// <summary>
///     Allows us to walk the syntax tree and determine the visualization state
/// </summary>
internal class VisualizationStateWalker
{
    public VisualizationState State { get; private set; } = VisualizationState.Empty;
    public int TextStart { get; private set; }
    public int TextLength { get; private set; }

    public void Walk(KustoCode code)
    {
        SyntaxElement.WalkNodes(
            code.Syntax,
            node =>
            {
                if (node is RenderOperator render)
                    HandleRenderOperator(render);
            },
            node => { });
    }

    private void HandleRenderOperator(RenderOperator node)
    {
        var chartType = node.ChartType.Text;
        var items = new Dictionary<string, string>();

        GetEnclosingPipeRange(node);
        if (node.WithClause == null)
        {
            State = new VisualizationState(chartType, ImmutableDictionary<string, string>.Empty);
            return;
        }

        foreach (var property in node.WithClause.Properties)
        {
            var element = property.Element;

            if (element.Expression is not LiteralExpression literalExpression)
                continue;
            var val = literalExpression.LiteralValue;
            items.Add(element.Name.SimpleName, val.ToString().NullToEmpty());
        }

        State = new VisualizationState(chartType,
            items.ToImmutableDictionary());
    }

    private void GetEnclosingPipeRange(SyntaxNode node)
    {

        
        if (node.Parent is PipeExpression expr)
        {
            var element =expr.GetChild(node.IndexInParent);
            TextStart = expr.TextStart + expr.Expression.End;
            TextLength= expr.End-TextStart;
        }
        else
        {
            TextStart = node.TextStart;
            TextLength = node.End - TextStart;
        }
    }

    public string RemoveRenderFromQuery(string query)
    {
       if (TextLength==0) return query;
       return query[..TextStart] + query[(TextStart+TextLength) ..];
    }
}
