// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation
{
    internal abstract class DefaultIRNodeVisitor<TResult, TContext> : IRNodeVisitor<TResult, TContext>
    {
        public virtual TResult? DefaultVisit(IRNode node, TContext context)
        {
            throw new InvalidOperationException($"Visitor method not implemented for {TypeNameHelper.GetTypeDisplayName(node)} ({TypeNameHelper.GetTypeDisplayName(this)})");
        }

        public override TResult? VisitAggregateCallNode(IRAggregateCallNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitBinaryExpression(IRBinaryExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitBuiltInScalarFunctionCall(IRBuiltInScalarFunctionCallNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitCastExpression(IRCastExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitDataTableExpression(IRDataTableExpression node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitExpressionStatement(IRExpressionStatementNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitFilterOperator(IRFilterOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitFunctionBody(IRFunctionBodyNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitFunctionDeclaration(IRFunctionDeclarationNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitJoinOperator(IRJoinOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitMaterializeExpression(IRMaterializeExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitLetStatement(IRLetStatementNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitList(IRListNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitLiteralExpression(IRLiteralExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitNameReference(IRNameReferenceNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitOrderedExpression(IROrderedExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitOutputColumn(IROutputColumnNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitPipeExpression(IRPipeExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitPrintOperator(IRPrintOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitProjectOperator(IRProjectOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitQueryBlock(IRQueryBlockNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitRowScopeNameReferenceNode(IRRowScopeNameReferenceNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitSortOperator(IRSortOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitSummarizeOperator(IRSummarizeOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitTakeOperator(IRTakeOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitToScalarExpressionNode(IRToScalarExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitUnaryExpression(IRUnaryExpressionNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitUnionOperator(IRUnionOperatorNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitUserFunctionCall(IRUserFunctionCallNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }

        public override TResult? VisitBuiltInWindowFunctionCall(IRBuiltInWindowFunctionCallNode node, TContext context)
        {
            return this.DefaultVisit(node, context);
        }
    }
}
