//
// Licensed under the MIT License.

using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator : DefaultSyntaxVisitor<IRNode>
{
    public override IRNode VisitLetStatement(LetStatement node)
    {
        var expression = (IRExpressionNode)node.Expression.Accept(this);

        // Let statement introduces a new name into the current scope. We must add
        // the name here so that subsequent expressions know the result kind associated
        // with the symbol.
        // Test, UserDefinedFunction_With_Local_Variables, fails without the following line:
        SetInScopeSymbolInfo(node.Name.ReferencedSymbol.Name, expression.ResultKind);
        return new IRLetStatementNode(node.Name.ReferencedSymbol, expression);
    }

    public override IRNode VisitFunctionDeclaration(FunctionDeclaration node) =>
        new IRFunctionDeclarationNode(node.ResultType);

    public override IRNode VisitExpressionStatement(ExpressionStatement node)
    {
        var irExpression = node.Expression.Accept(this);
        return new IRExpressionStatementNode(irExpression);
    }
}