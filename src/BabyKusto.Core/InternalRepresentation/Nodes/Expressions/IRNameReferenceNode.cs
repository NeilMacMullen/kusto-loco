// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRNameReferenceNode : IRExpressionNode
    {
        public IRNameReferenceNode(Symbol referencedSymbol, TypeSymbol resultType, EvaluatedExpressionKind resultKind)
            : base(resultType, resultKind)
        {
            this.ReferencedSymbol = referencedSymbol ?? throw new ArgumentNullException(nameof(referencedSymbol));
        }

        public IRNameReferenceNode(Symbol referencedSymbol, TypeSymbol resultType)
            : this(referencedSymbol, resultType, GetResultKind(resultType))
        {
        }

        public Symbol ReferencedSymbol { get; }

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitNameReference(this, context);
        }

        public override string ToString()
        {
            return $"NameReference({ReferencedSymbol.Name}): {ResultType.Display}";
        }
    }
}
