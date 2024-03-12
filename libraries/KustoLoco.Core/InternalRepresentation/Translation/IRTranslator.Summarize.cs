// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Syntax;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitSummarizeOperator(SummarizeOperator node)
    {
        List<IRExpressionNode> byColumns = new();
        if (node.ByClause != null)
        {
            var byExpressionsCount = node.ByClause.Expressions.Count;
            for (var i = 0; i < byExpressionsCount; i++)
            {
                var expression = node.ByClause.Expressions[i].Element;
                var irExpression = (IRExpressionNode)expression.Accept(this);
                byColumns.Add(irExpression);
            }
        }

        var aggregations = node
            .Aggregates
            .Select(t => t.Element)
            .Select(expression => (IRExpressionNode)expression.Accept(this))
            .ToArray();

        return new IRSummarizeOperatorNode(IRListNode.From(aggregations), IRListNode.From(byColumns), node.ResultType);
    }
}