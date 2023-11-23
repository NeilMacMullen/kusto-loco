// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitFunctionBody(FunctionBody node)
    {
        var statements = new List<IRStatementNode>();
        foreach (var statement in node.Statements)
        {
            var irStatement = (IRStatementNode)statement.Element.Accept(this);
            statements.Add(irStatement);
        }

        var irExpression = (IRExpressionNode)node.Expression.Accept(this);

        return new IRFunctionBodyNode(IRListNode.From(statements), irExpression);
    }
}