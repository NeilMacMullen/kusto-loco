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
                // Get the actual expression being expanded (unwrap SimpleNamedExpression if present)
                var actualExpression = mvExpandExpr.Expression;
                if (actualExpression is SimpleNamedExpression namedExpr)
                {
                    actualExpression = namedExpr.Expression;
                }
                
                // Get the column being expanded
                var irExpression = (IRExpressionNode)actualExpression.Accept(this);
                
                // Find the corresponding column in the result type
                // The expanded column will be in the result type
                var resultType = (TableSymbol)node.ResultType;
                ColumnSymbol? expandedColumn = null;
                
                // Build the expected column name based on the expression type
                // For aliased expressions (alias = column), use the alias name
                var expectedColName = GetExpandedColumnName(mvExpandExpr.Expression);
                
                // Look for the column in the result type that corresponds to this expansion
                foreach (var member in resultType.Members)
                {
                    if (member is ColumnSymbol colSymbol)
                    {
                        if (colSymbol.Name == expectedColName)
                        {
                            expandedColumn = colSymbol;
                            break;
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

    /// <summary>
    /// Gets the expected column name for an mv-expand expression.
    /// For aliased expressions (alias = column), returns the alias name.
    /// For simple columns, this is the column name.
    /// For path expressions (e.g., properties.ipConfigurations), this is the path with dots replaced by underscores.
    /// </summary>
    private static string GetExpandedColumnName(Expression expression)
    {
        // Handle aliased expressions: alias = column
        if (expression is SimpleNamedExpression namedExpr)
        {
            return namedExpr.Name.SimpleName;
        }
        
        if (expression is NameReference nameRef)
        {
            return nameRef.SimpleName;
        }
        else if (expression is PathExpression pathExpr)
        {
            // Build the full path name by collecting all parts
            var parts = new List<string>();
            CollectPathParts(pathExpr, parts);
            return string.Join("_", parts);
        }
        
        // Fallback to the expression text
        return expression.ToString();
    }

    /// <summary>
    /// Recursively collects the parts of a path expression.
    /// </summary>
    private static void CollectPathParts(Expression expression, List<string> parts)
    {
        if (expression is PathExpression pathExpr)
        {
            CollectPathParts(pathExpr.Expression, parts);
            if (pathExpr.Selector is NameReference nameRef)
            {
                parts.Add(nameRef.SimpleName);
            }
        }
        else if (expression is NameReference nameRef)
        {
            parts.Add(nameRef.SimpleName);
        }
    }
}
