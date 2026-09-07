//
// Licensed under the MIT License.

using System;
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
        var resultType = (TableSymbol)node.ResultType;
        var withItemIndexColumn = GetWithItemIndexColumn(node.Parameters, resultType);
        
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
                ColumnSymbol? expandedColumn = null;
                
                // Build the expected column name based on the expression type
                // For aliased expressions (alias = column), use the alias name
                var expectedColName = GetExpandedColumnName(mvExpandExpr.Expression);
                
                // Look for the column in the result type that corresponds to this expansion
                foreach (var colSymbol in resultType.Columns)
                    if (colSymbol.Name == expectedColName)
                    {
                        expandedColumn = colSymbol;
                        break;
                    }
                
                if (expandedColumn != null)
                {
                    var outputType = GetMvExpandOutputType(mvExpandExpr);
                    columns.Add(new IRMvExpandColumnNode(new ColumnSymbol(expandedColumn.Name, outputType),
                        irExpression));
                }
            }
        }
        
        return new IRMvExpandOperatorNode(columns, withItemIndexColumn, node.ResultType);
    }

    private static TypeSymbol GetMvExpandOutputType(MvExpandExpression mvExpandExpression)
    {
        TypeSymbol? elementType = null;
        var typeArgs = mvExpandExpression.ToTypeOf?.TypeOf?.Types;
        if (typeArgs is { Count: > 0 } && typeArgs[0].Element is { } typeExpr)
            elementType = typeExpr.ReferencedSymbol as TypeSymbol;

        return elementType ?? ScalarTypes.Dynamic;
    }

    private static ColumnSymbol? GetWithItemIndexColumn(SyntaxList<NamedParameter> parameters, TableSymbol resultType)
    {
        foreach (var parameter in parameters)
        {
            if (!string.Equals(parameter.Name.SimpleName, "with_itemindex", StringComparison.OrdinalIgnoreCase))
                continue;

            var itemIndexColumnName = parameter.Expression switch
            {
                NameDeclaration nameDeclaration => nameDeclaration.SimpleName,
                NameReference nameReference => nameReference.SimpleName,
                LiteralExpression { LiteralValue: string literalName } => literalName,
                _ => throw new InvalidOperationException(
                    $"Expected with_itemindex to specify a column name, found {parameter.Expression}")
            };

            foreach (var colSymbol in resultType.Columns)
                if (colSymbol.Name == itemIndexColumnName)
                    return colSymbol;

            throw new InvalidOperationException(
                $"Could not find mv-expand with_itemindex column '{itemIndexColumnName}' in result schema.");
        }

        return null;
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
