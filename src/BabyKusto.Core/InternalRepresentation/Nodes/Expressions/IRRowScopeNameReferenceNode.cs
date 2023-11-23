// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRRowScopeNameReferenceNode : IRExpressionNode
{
    public readonly int ReferencedColumnIndex;

    public readonly Symbol ReferencedSymbol;

    public IRRowScopeNameReferenceNode(Symbol referencedSymbol, TypeSymbol resultType, int referencedColumnIndex)
        : base(resultType, EvaluatedExpressionKind.Columnar)
    {
        if (referencedColumnIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(referencedColumnIndex));
        }

        ReferencedSymbol = referencedSymbol ?? throw new ArgumentNullException(nameof(referencedSymbol));
        ReferencedColumnIndex = referencedColumnIndex;
    }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitRowScopeNameReferenceNode(this, context);

    public override string ToString() =>
        $"RowScopeNameReferenceNode({ReferencedSymbol.Name}=[{ReferencedColumnIndex}]): {SchemaDisplay.GetText(ResultType)}";
}