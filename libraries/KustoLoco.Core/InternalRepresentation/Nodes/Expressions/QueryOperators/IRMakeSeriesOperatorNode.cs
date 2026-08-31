//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

// make-series Aggregations [default=..] on Axis from From to To step Step by ByColumns: groups rows by ByColumns,
// bins the Axis into [From, To) buckets of width Step, aggregates within each bucket, gap-fills empty buckets with the
// per-aggregate default, and returns one row per group whose Axis and aggregate columns are dynamic-array series.
internal class IRMakeSeriesOperatorNode : IRQueryOperatorNode
{
    public IRMakeSeriesOperatorNode(
        List<IRExpressionNode> aggregations,
        List<object?> defaults,
        List<bool> defaultProvided,
        IRExpressionNode axis,
        IRExpressionNode from,
        IRExpressionNode to,
        IRExpressionNode step,
        List<IRExpressionNode> byColumns,
        bool stopInclusive,
        TypeSymbol resultType)
        : base(resultType)
    {
        Aggregations = aggregations ?? throw new ArgumentNullException(nameof(aggregations));
        Defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        DefaultProvided = defaultProvided ?? throw new ArgumentNullException(nameof(defaultProvided));
        StopInclusive = stopInclusive;
        Axis = axis ?? throw new ArgumentNullException(nameof(axis));
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
        Step = step ?? throw new ArgumentNullException(nameof(step));
        ByColumns = byColumns ?? throw new ArgumentNullException(nameof(byColumns));

        _children = new List<IRNode>();
        _children.AddRange(aggregations);
        _children.Add(axis);
        _children.Add(from);
        _children.Add(to);
        _children.Add(step);
        _children.AddRange(byColumns);
    }

    private readonly List<IRNode> _children;

    public List<IRExpressionNode> Aggregations { get; }
    public List<object?> Defaults { get; }
    // Whether a 'default=' clause was written for each aggregate. Distinguishes "no default" (ADX fills gaps with a
    // typed 0) from an explicit 'default=<null>' (fills with null, e.g. for the series_fill_* interpolation functions).
    public List<bool> DefaultProvided { get; }
    public IRExpressionNode Axis { get; }
    public IRExpressionNode From { get; }
    public IRExpressionNode To { get; }
    public IRExpressionNode Step { get; }
    public List<IRExpressionNode> ByColumns { get; }
    // The alternate 'in range(start, stop, step)' syntax has an INCLUSIVE stop (unlike 'from .. to ..' where 'to' is
    // non-inclusive), so the last axis point can equal stop. (range() also bins with bin() rather than bin_at(); for the
    // usual step-aligned start the two coincide.)
    public bool StopInclusive { get; }

    public override int ChildCount => _children.Count;

    public override IRNode GetChild(int index) => _children[index];

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitMakeSeriesOperator(this, context);
}
