//
// Licensed under the MIT License.

using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;

namespace KustoLoco.Core.InternalRepresentation;

internal abstract class IRNodeVisitor<TResult, TContext>
{
    public abstract TResult VisitAggregateCallNode(IRAggregateCallNode node, TContext context);
    public abstract TResult VisitBinaryExpression(IRBinaryExpressionNode node, TContext context);
    public abstract TResult VisitBuiltInScalarFunctionCall(IRBuiltInScalarFunctionCallNode node, TContext context);
    public abstract TResult VisitCastExpression(IRCastExpressionNode node, TContext context);
    public abstract TResult VisitDataTableExpression(IRDataTableExpression node, TContext context);
    public abstract TResult VisitExpressionStatement(IRExpressionStatementNode node, TContext context);
    public abstract TResult VisitFilterOperator(IRFilterOperatorNode node, TContext context);
    public abstract TResult VisitFunctionBody(IRFunctionBodyNode node, TContext context);
    public abstract TResult VisitFunctionDeclaration(IRFunctionDeclarationNode node, TContext context);
    public abstract TResult VisitJoinOperator(IRJoinOperatorNode node, TContext context);
    public abstract TResult VisitMaterializeExpression(IRMaterializeExpressionNode node, TContext context);
    public abstract TResult VisitLetStatement(IRLetStatementNode node, TContext context);
    public abstract TResult VisitList(IRListNode node, TContext context);
    public abstract TResult VisitLiteralExpression(IRLiteralExpressionNode node, TContext context);
    public abstract TResult VisitMemberAccess(IRMemberAccessNode node, TContext context);
    public abstract TResult VisitMemberAccess(IRArrayAccessNode node, TContext context);
    public abstract TResult VisitNameReference(IRNameReferenceNode node, TContext context);
    public abstract TResult VisitOrderedExpression(IROrderedExpressionNode node, TContext context);
    public abstract TResult VisitOutputColumn(IROutputColumnNode node, TContext context);
    public abstract TResult VisitPipeExpression(IRPipeExpressionNode node, TContext context);
    public abstract TResult VisitPrintOperator(IRPrintOperatorNode node, TContext context);
    public abstract TResult VisitProjectOperator(IRProjectOperatorNode node, TContext context);
    public abstract TResult VisitQueryBlock(IRQueryBlockNode node, TContext context);
    public abstract TResult VisitRenderOperator(IRRenderOperatorNode node, TContext context);
    public abstract TResult VisitRowScopeNameReferenceNode(IRRowScopeNameReferenceNode node, TContext context);
    public abstract TResult VisitSortOperator(IRSortOperatorNode node, TContext context);
    public abstract TResult VisitTopOperator(IRTopOperatorNode node, TContext context);
    public abstract TResult VisitSummarizeOperator(IRSummarizeOperatorNode node, TContext context);
    public abstract TResult VisitTakeOperator(IRTakeOperatorNode node, TContext context);
    public abstract TResult VisitGetSchemaOperator(IRGetSchemaOperatorNode node, TContext context);
    public abstract TResult VisitToScalarExpressionNode(IRToScalarExpressionNode node, TContext context);
    public abstract TResult VisitUnaryExpression(IRUnaryExpressionNode node, TContext context);
    public abstract TResult VisitUnionOperator(IRUnionOperatorNode node, TContext context);
    public abstract TResult VisitUserFunctionCall(IRUserFunctionCallNode node, TContext context);
    public abstract TResult VisitBuiltInWindowFunctionCall(IRBuiltInWindowFunctionCallNode node, TContext context);
    public abstract TResult VisitRangeOperator(IRRangeOperatorNode node, TContext context);
    public abstract TResult VisitStarExpression(IRStarExpression node, TContext context);
    public abstract TResult VisitMvExpandOperator(IRMvExpandOperatorNode node, TContext context);

}
