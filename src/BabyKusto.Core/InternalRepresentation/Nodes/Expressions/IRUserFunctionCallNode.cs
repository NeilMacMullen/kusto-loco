// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation
{
    internal class IRUserFunctionCallNode : IRExpressionNode
    {
        public IRUserFunctionCallNode(Signature signature, IReadOnlyList<Parameter> parameters, IReadOnlyList<VariableSymbol> paramSymbols, IRFunctionBodyNode expandedBody, IRListNode<IRExpressionNode> arguments, TypeSymbol resultType)
            : base(resultType, GetResultKind(arguments))
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ParamSymbols = paramSymbols ?? throw new ArgumentNullException();
            ExpandedBody = expandedBody ?? throw new ArgumentNullException(nameof(expandedBody));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public Signature Signature { get; }
        public IReadOnlyList<Parameter> Parameters { get; }

        // The intention was to have these symbols represent the exact same symbols that appear in the expanded body but that wasn't successful...
        public IReadOnlyList<VariableSymbol> ParamSymbols { get; }
        public IRFunctionBodyNode ExpandedBody { get; }
        public IRListNode<IRExpressionNode> Arguments { get; }

        public override int ChildCount => 2;
        public override IRNode GetChild(int index) =>
            index switch
            {
                0 => ExpandedBody,
                1 => Arguments,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

        public override TResult? Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
            where TResult : class
        {
            return visitor.VisitUserFunctionCall(this, context);
        }

        public override string ToString()
        {
            return $"UserFunctionCall({Signature.Display}): {ResultType.Display}";
        }
    }
}
