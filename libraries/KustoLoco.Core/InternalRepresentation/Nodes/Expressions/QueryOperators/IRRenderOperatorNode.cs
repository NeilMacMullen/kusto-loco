//
// Licensed under the MIT License.

using System;
using System.Collections.Immutable;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRRenderOperatorNode : IRQueryOperatorNode
{
    public IRRenderOperatorNode(string chartType, ImmutableDictionary<string, string> items, TypeSymbol resultType)
        : base(resultType)
    {
        ChartType = chartType;
        Items = items;
    }

    public string ChartType { get; }
    public ImmutableDictionary<string, string> Items { get; }

    public override int ChildCount => 0;
    public override IRNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitRenderOperator(this, context);

    public override string ToString() => "RenderOperator";
}
