﻿//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRNameReferenceNode : IRExpressionNode
{
    public IRNameReferenceNode(Symbol referencedSymbol, TypeSymbol resultType, EvaluatedExpressionKind resultKind)
        : base(resultType, resultKind) =>
        ReferencedSymbol = referencedSymbol ?? throw new ArgumentNullException(nameof(referencedSymbol));

    public IRNameReferenceNode(Symbol referencedSymbol, TypeSymbol resultType)
        : this(referencedSymbol, resultType, GetResultKind(resultType))
    {
    }

    public Symbol ReferencedSymbol { get; }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitNameReference(this, context);

    public override string ToString() =>
        $"NameReference({ReferencedSymbol.Name}): {SchemaDisplay.GetText(ResultType)}";
}