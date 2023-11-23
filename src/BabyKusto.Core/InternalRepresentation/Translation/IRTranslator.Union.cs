// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitUnionOperator(UnionOperator node)
    {
        var expressions = new List<IRExpressionNode>();
        foreach (var expression in node.Expressions)
        {
            var irExpression = (IRExpressionNode)expression.Element.Accept(this);
            expressions.Add(irExpression);
        }

        return new IRUnionOperatorNode(IRListNode.From(expressions), node.ResultType);
    }
}