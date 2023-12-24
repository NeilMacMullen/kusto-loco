// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Kusto.Language.Syntax;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitRenderOperator(RenderOperator node)
    {
        var chartType = node.ChartType.Text;
        var items = new Dictionary<string, object>();
        if (node.WithClause != null)
        {
            foreach (var prop in node.WithClause.Properties)
            {
                var element = prop.Element;


                var literalExpression = element.Expression as LiteralExpression;
                if (literalExpression == null)
                {
                    throw new InvalidOperationException(
                        $"Expected render operator with-clause property expression to be {TypeNameHelper.GetTypeDisplayName(typeof(LiteralExpression))}, but found {TypeNameHelper.GetTypeDisplayName(element.Expression)}");
                }

                var val = literalExpression.LiteralValue;
                items.Add(element.Name.SimpleName, val);
            }
        }

        return new IRRenderOperatorNode(chartType: chartType,
            items: items.ToImmutableDictionary(),
            node.ResultType);
    }
}