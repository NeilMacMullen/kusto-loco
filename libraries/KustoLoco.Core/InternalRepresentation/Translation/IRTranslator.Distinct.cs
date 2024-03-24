// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

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
            irExpressions.AddRange(table.Columns
                .Select((column, i) => new IRRowScopeNameReferenceNode(column, column.Type, i)));
        }
        else
        {
            irExpressions.AddRange(node.Expressions.Select(expression =>
                (IRExpressionNode)expression.Element.Accept(this)));
        }

        return new IRSummarizeOperatorNode(aggregations: IRListNode<IRExpressionNode>.Empty,
            byColumns: IRListNode.From(irExpressions), node.ResultType);
    }
}