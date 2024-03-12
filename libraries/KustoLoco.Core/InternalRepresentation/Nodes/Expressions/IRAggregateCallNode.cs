// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRAggregateCallNode : IRExpressionNode
{
    public IRAggregateCallNode(Signature signature, AggregateOverloadInfo overloadInfo,
        IReadOnlyList<Parameter> parameters, IRListNode<IRExpressionNode> arguments, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Scalar)
    {
        Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        OverloadInfo = overloadInfo ?? throw new ArgumentNullException(nameof(overloadInfo));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    public Signature Signature { get; }
    public AggregateOverloadInfo OverloadInfo { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public IRListNode<IRExpressionNode> Arguments { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Arguments,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitAggregateCallNode(this, context);

    public override string ToString() =>
        $"AggregateCall({SchemaDisplay.GetText(Signature.Symbol)}): {SchemaDisplay.GetText(ResultType)}";
}