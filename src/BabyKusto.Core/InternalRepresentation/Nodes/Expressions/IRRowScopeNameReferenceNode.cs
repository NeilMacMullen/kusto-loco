// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRRowScopeNameReferenceNode : IRExpressionNode
    {
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

        public Symbol ReferencedSymbol { get; }
        public int ReferencedColumnIndex { get; }

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitRowScopeNameReferenceNode(this, context);
        }

        public override string ToString()
        {
            return $"RowScopeNameReferenceNode({ReferencedSymbol.Name}=[{ReferencedColumnIndex}]): {ResultType.Display}";
        }
    }
}
