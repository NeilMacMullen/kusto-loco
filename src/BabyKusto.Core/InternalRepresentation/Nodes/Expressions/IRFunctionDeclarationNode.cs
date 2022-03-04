// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRFunctionDeclarationNode : IRExpressionNode
    {
        public IRFunctionDeclarationNode(TypeSymbol resultType)
            : base(resultType, EvaluatedExpressionKind.Scalar)
        {
        }

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitFunctionDeclaration(this, context);
        }

        public override string ToString()
        {
            return $"FunctionDeclaration: {ResultType.Display}";
        }
    }
}
