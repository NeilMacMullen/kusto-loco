// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator : DefaultSyntaxVisitor<IRNode>
    {
        public override IRNode VisitLetStatement(LetStatement node)
        {
            var expression = (IRExpressionNode)node.Expression.Accept(this);
            return new IRLetStatementNode(node.Name.ReferencedSymbol, expression);
        }

        public override IRNode VisitFunctionDeclaration(FunctionDeclaration node)
        {
            return new IRFunctionDeclarationNode(node.ResultType);
        }

        public override IRNode VisitExpressionStatement(ExpressionStatement node)
        {
            var irExpression = node.Expression.Accept(this);
            return new IRExpressionStatementNode(irExpression);
        }
    }
}
