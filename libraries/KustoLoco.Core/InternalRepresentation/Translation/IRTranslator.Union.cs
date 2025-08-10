﻿//
// Licensed under the MIT License.

using System.Linq;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitUnionOperator(UnionOperator node)
    {
        var expressions = node.Expressions
            .Select(expression => (IRExpressionNode)expression.Element.Accept(this))
            .ToArray();

        return new IRUnionOperatorNode(IRListNode.From(expressions), node.ResultType);
    }
}