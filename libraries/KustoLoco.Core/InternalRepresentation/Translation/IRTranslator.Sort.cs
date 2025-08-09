//
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitTopOperator(TopOperator node)
    {
        var expressions = new List<IROrderedExpressionNode>();
        var expression = node.ByExpression;
        var (sortDirection, nullsDirection, underlyingExpression) = SortHelper.GetDirection(expression);
        var irExpression = (IRExpressionNode)underlyingExpression.Accept(this);
        expressions.Add(new IROrderedExpressionNode(irExpression, sortDirection, nullsDirection, node.ResultType));
        var countExpr = (IRExpressionNode)node.Expression.Accept(this);
        return new IRTopOperatorNode(countExpr,expressions[0], node.ResultType);
    }

    public override IRNode VisitSortOperator(SortOperator node)
    {
        var expressions = new List<IROrderedExpressionNode>();
        foreach (var expression in node.Expressions)
        {
            var (sortDirection, nullsDirection, underlyingExpression)
                = SortHelper.GetDirection(expression.Element);
            var irExpression = (IRExpressionNode)underlyingExpression.Accept(this);
            expressions.Add(new IROrderedExpressionNode(irExpression, sortDirection, nullsDirection, node.ResultType));
        }

        return new IRSortOperatorNode(IRListNode.From(expressions), node.ResultType);
    }
}
