﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;

namespace BabyKusto.Core.Evaluation
{
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

        public override EvaluationResult VisitFunctionDeclaration(IRFunctionDeclarationNode node, EvaluationContext context)
        {
            // Kusto lib has already done the heavy lifting, we can skip...
            return EvaluationResult.Null;
        }

        public override EvaluationResult VisitExpressionStatement(IRExpressionStatementNode node, EvaluationContext context)
        {
            return node.Expression.Accept(this, context);
        }
    }
}
