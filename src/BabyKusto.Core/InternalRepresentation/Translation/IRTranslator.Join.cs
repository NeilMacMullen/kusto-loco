// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Syntax;

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
                            "rightanti" or "rightantisemi"  => IRJoinKind.RightAnti,
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
            List<IRExpressionNode> onExpressions = new();
            if (node.ConditionClause is JoinOnClause onClause)
            {
                foreach (var element in onClause.Expressions)
                {
                    var expression = element.Element;
                    onExpressions.Add((IRExpressionNode)expression.Accept(this));
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported join clause type: {node.ConditionClause.GetType().FullName}");
            }

            return new IRJoinOperatorNode(kind, irExpression, IRListNode.From(onExpressions), node.ResultType);
        }
    }
}
