// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitSortOperator(SortOperator node)
    {
        var expressions = new List<IROrderedExpressionNode>();
        foreach (var expression in node.Expressions)
        {
            Expression underlyingExpression;
            SortDirections sortDirection;
            NullsDirections nullsDirection;

            if (expression.Element is OrderedExpression orderedExpression)
            {
                var ordering = orderedExpression.Ordering;
                sortDirection = ordering.AscOrDescKeyword?.Kind switch
                {
                    null => SortDirections.Desc,
                    SyntaxKind.AscKeyword => SortDirections.Asc,
                    SyntaxKind.DescKeyword => SortDirections.Desc,
                    SyntaxKind.GrannyAscKeyword => SortDirections.GrannyAsc,
                    SyntaxKind.GrannyDescKeyword => SortDirections.GrannyDesc,
                    _ => throw new InvalidOperationException(
                        $"Unexpected ordering kind {ordering.AscOrDescKeyword.Kind}"),
                };
                nullsDirection = ordering.NullsClause?.FirstOrLastKeyword.Kind switch
                {
                    null => (sortDirection == SortDirections.Asc || sortDirection == SortDirections.GrannyAsc)
                        ? NullsDirections.First
                        : NullsDirections.Last,
                    SyntaxKind.FirstKeyword => NullsDirections.First,
                    SyntaxKind.LastKeyword => NullsDirections.Last,
                    _ => throw new InvalidOperationException(
                        $"Unexpected nulls ordering kind {ordering.NullsClause.Kind}"),
                };
                underlyingExpression = orderedExpression.Expression;
            }
            else
            {
                sortDirection = SortDirections.Desc;
                nullsDirection = NullsDirections.Last;
                underlyingExpression = expression.Element;
            }

            var irExpression = (IRExpressionNode)underlyingExpression.Accept(this);

            expressions.Add(new IROrderedExpressionNode(irExpression, sortDirection, nullsDirection, node.ResultType));
        }

        return new IRSortOperatorNode(IRListNode.From(expressions), node.ResultType);
    }
}