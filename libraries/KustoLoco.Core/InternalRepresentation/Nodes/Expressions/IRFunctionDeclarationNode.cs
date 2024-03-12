// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation;

internal class IRFunctionDeclarationNode : IRExpressionNode
{
    public IRFunctionDeclarationNode(TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Scalar)
    {
    }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitFunctionDeclaration(this, context);

    public override string ToString() => $"FunctionDeclaration: {SchemaDisplay.GetText(ResultType)}";
}