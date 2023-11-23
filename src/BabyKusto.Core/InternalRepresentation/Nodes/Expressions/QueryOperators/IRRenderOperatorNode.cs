// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRRenderOperatorNode : IRQueryOperatorNode
{
    public IRRenderOperatorNode(string chartType, string? kind, TypeSymbol resultType)
        : base(resultType)
    {
        ChartType = chartType;
        Kind = kind;
    }

    public string ChartType { get; }
    public string? Kind { get; }

    public override int ChildCount => 0;
    public override IRNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        where TResult : class =>
        visitor.VisitRenderOperator(this, context);

    public override string ToString() => "RenderOperator";
}