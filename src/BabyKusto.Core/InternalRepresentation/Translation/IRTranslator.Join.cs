// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Kusto.Language.Utils;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitJoinOperator(JoinOperator node)
        {
            var kind = IRJoinKind.InnerUnique;
            foreach (var parameter in node.Parameters)
            {
                if (parameter.Name.SimpleName == "kind")
                {
                    if (parameter.Expression is LiteralExpression literalExpression)
                    {
                        kind = literalExpression.LiteralValue switch
                        {
                            "innerunique" => IRJoinKind.InnerUnique,
                            "inner" => IRJoinKind.Inner,
                            "leftouter" => IRJoinKind.LeftOuter,
                            "rightouter" => IRJoinKind.RightOuter,
                            "fullouter" => IRJoinKind.FullOuter,
                            "leftsemi" => IRJoinKind.LeftSemi,
                            "rightsemi" => IRJoinKind.RightSemi,
                            "leftanti" or "anti" or "leftantisemi" => IRJoinKind.LeftAnti,
                            "rightanti" or "rightantisemi" => IRJoinKind.RightAnti,
                            _ => throw new NotSupportedException($"Unsupported join kind '{literalExpression.LiteralValue}'."),
                        };
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported join kind expression '{parameter.Expression?.GetType().FullName}', expected literal.");
                    }
                }
            }
            var irExpression = (IRExpressionNode)node.Expression.Accept(this);

            var rightType = irExpression.ResultType as TableSymbol;
            if (rightType == null)
            {
                throw new InvalidOperationException($"Expected join operator's Right type to be tabular, found {SchemaDisplay.GetText(irExpression.ResultType)}");
            }

            List<IRJoinOnClause> onExpressions = new();
            if (node.ConditionClause is JoinOnClause onClause)
            {
                Debug.Assert(_rowScope != null);
                foreach (var element in onClause.Expressions)
                {
                    var expression = element.Element;

                    IRRowScopeNameReferenceNode leftClause;
                    IRRowScopeNameReferenceNode rightClause;
                    if (expression is NameReference nameReferenceExpression)
                    {
                        var name = nameReferenceExpression.Name.SimpleName;

                        var leftIndex = _rowScope.Columns.FirstIndex(c => c.Name == name);
                        if (leftIndex < 0)
                        {
                            throw new InvalidOperationException($"Could not find column {name} on left side of join.");
                        }

                        var rightIndex = rightType.Columns.LastIndex(c => c.Name == name);
                        if (rightIndex < 0)
                        {
                            throw new InvalidOperationException($"Could not find column {name} on right side of join.");
                        }

                        leftClause = new IRRowScopeNameReferenceNode(_rowScope.Columns[leftIndex], _rowScope.Columns[leftIndex].Type, leftIndex);
                        rightClause = new IRRowScopeNameReferenceNode(rightType.Columns[rightIndex], rightType.Columns[rightIndex].Type, rightIndex);
                    }
                    else
                    {
                        if (expression is not BinaryExpression binaryExpression)
                        {
                            throw new InvalidOperationException($"Expected a binary expression for the join on clause, found {TypeNameHelper.GetTypeDisplayName(expression)}.");
                        }

                        if (binaryExpression.Kind != SyntaxKind.EqualExpression)
                        {
                            throw new InvalidOperationException($"Expected a binary equality expression for the join on clause, found {binaryExpression.Kind}.");
                        }

                        // TODO: Is it acceptable for the join on clause to swap $left and $right? E.g. `$right.a == $left.b`
                        if (binaryExpression.Left is not PathExpression leftPathExpression ||
                            leftPathExpression.Expression is not NameReference leftPathDollarLeftExpression ||
                            leftPathDollarLeftExpression.Name.SimpleName != "$left" ||
                            leftPathExpression.Selector is not NameReference leftNameReferenceExpression ||

                            binaryExpression.Right is not PathExpression rightPathExpression ||
                            rightPathExpression.Expression is not NameReference rightPathDollarLeftExpression ||
                            rightPathDollarLeftExpression.Name.SimpleName != "$right" ||
                            rightPathExpression.Selector is not NameReference rightNameReferenceExpression)
                        {
                            throw new InvalidOperationException($"Expected binary expression in join on clause to be made up of simple expressions like '$left.LeftColumn == $right.RightColumn', found {binaryExpression}");
                        }

                        var leftName = leftNameReferenceExpression.Name.SimpleName;
                        var leftIndex = _rowScope.Columns.FirstIndex(c => c.Name == leftName);
                        if (leftIndex < 0)
                        {
                            throw new InvalidOperationException($"Could not find column {leftName} on left side of join.");
                        }

                        var rightName = rightNameReferenceExpression.Name.SimpleName;
                        var rightIndex = rightType.Columns.LastIndex(c => c.Name == rightName);
                        if (rightIndex < 0)
                        {
                            throw new InvalidOperationException($"Could not find column {rightName} on right side of join.");
                        }

                        leftClause = new IRRowScopeNameReferenceNode(_rowScope.Columns[leftIndex], _rowScope.Columns[leftIndex].Type, leftIndex);
                        rightClause = new IRRowScopeNameReferenceNode(rightType.Columns[rightIndex], rightType.Columns[rightIndex].Type, rightIndex);
                    }

                    var clause = new IRJoinOnClause(leftClause, rightClause);
                    onExpressions.Add(clause);
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported join clause type: {node.ConditionClause.GetType().FullName}");
            }

            return new IRJoinOperatorNode(kind, irExpression, onExpressions, node.ResultType);
        }
    }
}
