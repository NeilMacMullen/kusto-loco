// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Syntax;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitRenderOperator(RenderOperator node)
    {
        var chartType = node.ChartType.Text;
        string? kind = null;

        if (node.WithClause != null)
        {
            foreach (var prop in node.WithClause.Properties)
            {
                var element = prop.Element;
                if (element.Name.SimpleName == "kind")
                {
                    var literalExpression = element.Expression as LiteralExpression;
                    if (literalExpression == null)
                    {
                        throw new InvalidOperationException(
                            $"Expected render operator with-clause property expression to be {TypeNameHelper.GetTypeDisplayName(typeof(LiteralExpression))}, but found {TypeNameHelper.GetTypeDisplayName(element.Expression)}");
                    }

                    kind = (string?)literalExpression.LiteralValue;
                }
            }
        }

        return new IRRenderOperatorNode(chartType: chartType, kind: kind, node.ResultType);
    }
}