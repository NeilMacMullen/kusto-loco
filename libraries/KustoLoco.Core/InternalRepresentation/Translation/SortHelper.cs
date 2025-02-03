using System;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

namespace KustoLoco.Core.InternalRepresentation;

internal static class SortHelper
{
    internal static (SortDirections sortDirection, NullsDirections nullDirection,
        Expression underlyingExpression)
        GetDirection(Expression expression)
    {
        SortDirections sortDirection;
        NullsDirections nullsDirection;
        Expression underlyingExpression;

        if (expression is OrderedExpression orderedExpression)
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
                    $"Unexpected ordering kind {ordering.AscOrDescKeyword.Kind}")
            };
            nullsDirection = ordering.NullsClause?.FirstOrLastKeyword.Kind switch
            {
                null => sortDirection == SortDirections.Asc || sortDirection == SortDirections.GrannyAsc
                    ? NullsDirections.First
                    : NullsDirections.Last,
                SyntaxKind.FirstKeyword => NullsDirections.First,
                SyntaxKind.LastKeyword => NullsDirections.Last,
                _ => throw new InvalidOperationException(
                    $"Unexpected nulls ordering kind {ordering.NullsClause.Kind}")
            };
            underlyingExpression = orderedExpression.Expression;
        }
        else
        {
            sortDirection = SortDirections.Desc;
            nullsDirection = NullsDirections.Last;
            underlyingExpression = expression;
        }

        return (sortDirection, nullsDirection,underlyingExpression);
    }
}
