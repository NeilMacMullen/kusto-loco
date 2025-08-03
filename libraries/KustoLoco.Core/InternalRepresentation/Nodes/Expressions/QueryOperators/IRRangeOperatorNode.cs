//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRRangeOperatorNode : IRQueryOperatorNode
{
    public IRRangeOperatorNode(Symbol columnNameSymbol,
        IRExpressionNode fromExpression,
        IRExpressionNode toExpression,
        IRExpressionNode stepExpression,
        TypeSymbol resultType)
        : base(resultType)
    {
        ColumnNameSymbol = columnNameSymbol;
        FromExpression = fromExpression;
        ToExpression = toExpression;
        StepExpression = stepExpression;
    }

    public Symbol ColumnNameSymbol { get; }
    public IRExpressionNode FromExpression { get; }
    public IRExpressionNode ToExpression { get; }
    public IRExpressionNode StepExpression { get; }

    public override int ChildCount => 3;

    public override IRNode GetChild(int index)
        => index switch
        {
            0 => FromExpression,
            1 => ToExpression,
            2 => StepExpression,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        where TResult : class
        => visitor.VisitRangeOperator(this, context);

    public override string ToString()
        => $"RangeOperator({ColumnNameSymbol.Name}): {SchemaDisplay.GetText(ResultType)}";
}
