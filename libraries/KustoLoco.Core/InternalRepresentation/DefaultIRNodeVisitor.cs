//
// Licensed under the MIT License.

using System;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;
using KustoLoco.Core.Util;


namespace KustoLoco.Core.InternalRepresentation;

internal abstract class DefaultIRNodeVisitor<TResult, TContext> : IRNodeVisitor<TResult, TContext>
{
    public virtual TResult DefaultVisit(IRNode node, TContext context) => throw new InvalidOperationException(
        $"Visitor method not implemented for {TypeNameHelper.GetTypeDisplayName(node)} ({TypeNameHelper.GetTypeDisplayName(this)})");

    public override TResult VisitAggregateCallNode(IRAggregateCallNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitBinaryExpression(IRBinaryExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult
        VisitBuiltInScalarFunctionCall(IRBuiltInScalarFunctionCallNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitCastExpression(IRCastExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitDataTableExpression(IRDataTableExpression node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitExpressionStatement(IRExpressionStatementNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitFilterOperator(IRFilterOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitFunctionBody(IRFunctionBodyNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitFunctionDeclaration(IRFunctionDeclarationNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitJoinOperator(IRJoinOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitMaterializeExpression(IRMaterializeExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitLetStatement(IRLetStatementNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitList(IRListNode node, TContext context) => DefaultVisit(node, context);

    public override TResult VisitLiteralExpression(IRLiteralExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitMemberAccess(IRMemberAccessNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitNameReference(IRNameReferenceNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitOrderedExpression(IROrderedExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitOutputColumn(IROutputColumnNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitPipeExpression(IRPipeExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitPrintOperator(IRPrintOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitProjectOperator(IRProjectOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitQueryBlock(IRQueryBlockNode node, TContext context) => DefaultVisit(node, context);

    public override TResult VisitRenderOperator(IRRenderOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitRowScopeNameReferenceNode(IRRowScopeNameReferenceNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitSortOperator(IRSortOperatorNode node, TContext context) =>
        DefaultVisit(node, context);
    public override TResult VisitTopOperator(IRTopOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitSummarizeOperator(IRSummarizeOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitTakeOperator(IRTakeOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitGetSchemaOperator(IRGetSchemaOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitToScalarExpressionNode(IRToScalarExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitUnaryExpression(IRUnaryExpressionNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitUnionOperator(IRUnionOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitUserFunctionCall(IRUserFunctionCallNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult
        VisitBuiltInWindowFunctionCall(IRBuiltInWindowFunctionCallNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitRangeOperator(IRRangeOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitStarExpression(IRStarExpression node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitMemberAccess(IRArrayAccessNode node, TContext context) =>
        DefaultVisit(node, context);

    public override TResult VisitMvExpandOperator(IRMvExpandOperatorNode node, TContext context) =>
        DefaultVisit(node, context);

}
