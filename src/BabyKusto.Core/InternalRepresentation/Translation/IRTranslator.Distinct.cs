// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitDistinctOperator(DistinctOperator node)
    {
        var irExpressions = new List<IRExpressionNode>();
        if (node.Expressions.Any(e => e.Element is StarExpression))
        {
            if (node.Expressions.Count != 1)
            {
                throw new InvalidOperationException("Expected 'distinct' operator with at most one '*' expression.");
            }

            var table = (TableSymbol)node.ResultType;
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                irExpressions.Add(new IRRowScopeNameReferenceNode(column, column.Type, i));
            }
        }
        else
        {
            foreach (var expression in node.Expressions)
            {
                irExpressions.Add((IRExpressionNode)expression.Element.Accept(this));
            }
        }

        return new IRSummarizeOperatorNode(aggregations: IRListNode<IRExpressionNode>.Empty,
            byColumns: IRListNode.From(irExpressions), node.ResultType);
    }
}