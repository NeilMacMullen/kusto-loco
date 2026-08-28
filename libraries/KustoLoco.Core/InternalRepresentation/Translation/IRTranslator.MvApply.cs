//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitMvApplyOperator(MvApplyOperator node)
    {
        var inputScope = _rowScope;
        var columns = new List<IRMvExpandColumnNode>();
        foreach (var element in node.Expressions)
        {
            if (element.Element is not MvApplyExpression mvApplyExpr) continue;

            var actualExpression = mvApplyExpr.Expression;
            if (actualExpression is SimpleNamedExpression namedExpr)
                actualExpression = namedExpr.Expression;

            // The source array expression resolves against the input row scope.
            var irExpression = (IRExpressionNode)actualExpression.Accept(this);
            var colName = GetExpandedColumnName(mvApplyExpr.Expression);

            // 'to typeof(T)': T is a type expression, so its ReferencedSymbol (not ResultType) is the element type.
            TypeSymbol? elementType = null;
            var typeArgs = mvApplyExpr.ToTypeOf?.TypeOf?.Types;
            if (typeArgs is { Count: > 0 } && typeArgs[0].Element is { } typeExpr)
                elementType = typeExpr.ReferencedSymbol as TypeSymbol;
            elementType ??= actualExpression.ResultType as TypeSymbol ?? ScalarTypes.Dynamic;

            columns.Add(new IRMvExpandColumnNode(new ColumnSymbol(colName, elementType), irExpression));
        }

        // Translate the subquery with the intermediate schema (input columns, with expanded columns retyped to their
        // element type) as the row scope, so the subquery's column references resolve against the expanded rows.
        var expandByName = columns.ToDictionary(c => c.ColumnSymbol.Name, c => c.ColumnSymbol);
        var interColumns = inputScope.Columns
            .Select(c => expandByName.TryGetValue(c.Name, out var ec) ? ec : c)
            .ToArray();

        var oldRowScope = _rowScope;
        _rowScope = new TableSymbol(interColumns);
        var subquery = (IRExpressionNode)node.Subquery.Expression.Accept(this);
        _rowScope = oldRowScope;

        long? rowLimit = null;
        if (node.RowLimitClause?.RowLimit is LiteralExpression { LiteralValue: long limit })
            rowLimit = limit;

        return new IRMvApplyOperatorNode(columns, subquery, rowLimit, node.ResultType);
    }
}
