//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitMakeSeriesOperator(MakeSeriesOperator node)
    {
        var aggregations = new List<IRExpressionNode>();
        var defaults = new List<object?>();
        foreach (var aggElement in node.Aggregates)
        {
            if (aggElement.Element is not MakeSeriesExpression mse) continue;
            aggregations.Add((IRExpressionNode)mse.Expression.Accept(this));
            object? def = null;
            if (mse.DefaultExpression?.Expression.Accept(this) is IRLiteralExpressionNode { Value: { } v })
                def = v;
            defaults.Add(def);
        }

        var axis = (IRExpressionNode)node.OnClause.Expression.Accept(this);

        IRExpressionNode from, to, step;
        if (node.RangeClause is MakeSeriesFromToStepClause fts)
        {
            from = (IRExpressionNode)(fts.MakeSeriesFromClause?.Expression
                ?? throw new NotImplementedException("make-series requires an explicit 'from'")).Accept(this);
            to = (IRExpressionNode)(fts.MakeSeriesToClause?.Expression
                ?? throw new NotImplementedException("make-series requires an explicit 'to'")).Accept(this);
            step = (IRExpressionNode)(fts.MakeSeriesStepClause?.Expression
                ?? throw new NotImplementedException("make-series requires an explicit 'step'")).Accept(this);
        }
        else if (node.RangeClause is MakeSeriesInRangeClause ir)
        {
            var args = ir.Arguments.Expressions;
            from = (IRExpressionNode)args[0].Element.Accept(this);
            to = (IRExpressionNode)args[1].Element.Accept(this);
            step = (IRExpressionNode)args[2].Element.Accept(this);
        }
        else
        {
            throw new NotImplementedException("make-series requires a 'from .. to .. step ..' or 'in_range(..)' range.");
        }

        var byColumns = new List<IRExpressionNode>();
        if (node.ByClause != null)
            foreach (var byElement in node.ByClause.Expressions)
                byColumns.Add((IRExpressionNode)byElement.Element.Accept(this));

        return new IRMakeSeriesOperatorNode(aggregations, defaults, axis, from, to, step, byColumns, node.ResultType);
    }
}
