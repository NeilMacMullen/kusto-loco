//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRMvExpandOperatorNode : IRQueryOperatorNode
{
    public IRMvExpandOperatorNode(List<IRMvExpandColumnNode> columns, TypeSymbol resultType)
        : base(resultType)
    {
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
    }

    public List<IRMvExpandColumnNode> Columns { get; }

    public override int ChildCount => Columns.Count;

    public override IRNode GetChild(int index)
    {
        if (index < 0 || index >= Columns.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return Columns[index].Expression;
    }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => visitor.VisitMvExpandOperator(this, context);

    public override string ToString() => $"MvExpandOperator: {SchemaDisplay.GetText(ResultType)}";
}

internal class IRMvExpandColumnNode
{
    public IRMvExpandColumnNode(ColumnSymbol columnSymbol, IRExpressionNode expression)
    {
        ColumnSymbol = columnSymbol ?? throw new ArgumentNullException(nameof(columnSymbol));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public ColumnSymbol ColumnSymbol { get; }
    public IRExpressionNode Expression { get; }
}
