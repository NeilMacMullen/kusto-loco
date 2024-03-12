// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRJoinOnClause
{
    public readonly IRRowScopeNameReferenceNode Left;
    public readonly IRRowScopeNameReferenceNode Right;

    public IRJoinOnClause(IRRowScopeNameReferenceNode left, IRRowScopeNameReferenceNode right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public override string ToString() =>
        $"JoinOnClause({SchemaDisplay.GetText(Left.ReferencedSymbol)}, {SchemaDisplay.GetText(Right.ReferencedSymbol)})";
}