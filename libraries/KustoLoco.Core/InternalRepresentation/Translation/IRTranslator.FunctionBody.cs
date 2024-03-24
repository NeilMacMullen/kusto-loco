// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitFunctionBody(FunctionBody node)
    {
        var statements = node.Statements
            .Select(statement => (IRStatementNode)statement.Element.Accept(this))
            .ToArray();

        var irExpression = (IRExpressionNode)node.Expression.Accept(this);

        return new IRFunctionBodyNode(IRListNode.From(statements), irExpression);
    }
}