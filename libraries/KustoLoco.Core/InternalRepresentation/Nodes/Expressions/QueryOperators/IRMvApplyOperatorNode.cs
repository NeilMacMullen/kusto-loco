//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

// mv-apply: expand the named columns (as mv-expand does) but, per source row, run the subquery pipeline over just that
// row's expanded rows and union the results. Columns reuse the mv-expand column node; Subquery is the pipeline to apply.
internal class IRMvApplyOperatorNode : IRQueryOperatorNode
{
    public IRMvApplyOperatorNode(List<IRMvExpandColumnNode> columns, IRExpressionNode subquery,
        long? rowLimit, TypeSymbol resultType)
        : base(resultType)
    {
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        Subquery = subquery ?? throw new ArgumentNullException(nameof(subquery));
        RowLimit = rowLimit;
    }

    public List<IRMvExpandColumnNode> Columns { get; }

    public IRExpressionNode Subquery { get; }

    public long? RowLimit { get; }

    public override int ChildCount => Columns.Count + 1;

    public override IRNode GetChild(int index) =>
        index < Columns.Count ? Columns[index].Expression : Subquery;

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitMvApplyOperator(this, context);
}
