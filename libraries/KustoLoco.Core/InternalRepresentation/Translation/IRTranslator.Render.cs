//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using KustoLoco.Core.Util;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using NotNullStrings;


namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitRenderOperator(RenderOperator node)
    {
        var chartType = node.ChartType.Text;
        var items = new Dictionary<string, string>();
        if (node.WithClause == null)
            return new IRRenderOperatorNode(chartType: chartType,
                items: ImmutableDictionary<string, string>.Empty, 
                node.ResultType);

        foreach (var prop in node.WithClause.Properties)
        {
            var element = prop.Element;

            if (element.Expression is not LiteralExpression literalExpression)
            {
                throw new InvalidOperationException(
                    $"Expected render operator with-clause property expression to be {TypeNameHelper.GetTypeDisplayName(typeof(LiteralExpression))}, but found {TypeNameHelper.GetTypeDisplayName(element.Expression)}");
            }

            var val = literalExpression.LiteralValue;
            items.Add(element.Name.SimpleName, val.ToString().NullToEmpty());
        }

        return new IRRenderOperatorNode(chartType: chartType,
            items: items.ToImmutableDictionary(),
            node.ResultType);
    }
}
