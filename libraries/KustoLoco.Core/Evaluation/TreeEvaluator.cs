// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator : DefaultIRNodeVisitor<EvaluationResult, EvaluationContext>
{
    public override EvaluationResult VisitQueryBlock(IRQueryBlockNode node, EvaluationContext context)
    {
        var lastResult = EvaluationResult.Null;
        for (var i = 0; i < node.Statements.ChildCount; i++)
        {
            var statement = node.Statements.GetChild(i);
            lastResult = statement.Accept(this, context);
        }

        return lastResult;
    }

    public override EvaluationResult VisitLetStatement(IRLetStatementNode node, EvaluationContext context)
    {
        var value = node.Expression.Accept(this, context);
        context.Scope.AddSymbol(node.Symbol, value);
        return EvaluationResult.Null;
    }

    public override EvaluationResult VisitFunctionDeclaration(IRFunctionDeclarationNode node,
        EvaluationContext context) =>
        // Kusto lib has already done the heavy lifting, we can skip...
        EvaluationResult.Null;

    public override EvaluationResult VisitExpressionStatement(IRExpressionStatementNode node,
        EvaluationContext context) => node.Expression.Accept(this, context);
}