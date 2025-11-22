//
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitMvExpandOperator(MvExpandOperator node)
    {
        var columns = new List<IRMvExpandColumnNode>();
        
        // Process each mv-expand expression
        for (var i = 0; i < node.Expressions.Count; i++)
        {
            var expression = node.Expressions[i].Element;
            
            if (expression is MvExpandExpression mvExpandExpr)
            {
                // Get the column being expanded
                var irExpression = (IRExpressionNode)mvExpandExpr.Expression.Accept(this);
                
                // Find the corresponding column in the result type
                // The expanded column will be in the result type
                var resultType = (TableSymbol)node.ResultType;
                ColumnSymbol? expandedColumn = null;
                
                // Look for the column in the result type that corresponds to this expansion
                foreach (var member in resultType.Members)
                {
                    if (member is ColumnSymbol colSymbol)
                    {
                        // The expanded column typically has the same name as the expression
                        // or is specified in the MvExpandExpression
                        if (mvExpandExpr.Expression is NameReference nameRef)
                        {
                            if (colSymbol.Name == nameRef.SimpleName)
                            {
                                expandedColumn = colSymbol;
                                break;
                            }
                        }
                    }
                }
                
                if (expandedColumn != null)
                {
                    columns.Add(new IRMvExpandColumnNode(expandedColumn, irExpression));
                }
            }
        }
        
        return new IRMvExpandOperatorNode(columns, node.ResultType);
    }
}
