using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRTopOperatorNode : IRQueryOperatorNode
{
    public IRTopOperatorNode(IRExpressionNode countExpression, IROrderedExpressionNode byExpression,
        TypeSymbol resultType)
        : base(resultType)
    {
        ByExpression = byExpression ?? throw new ArgumentNullException(nameof(byExpression));
        CountExpression = countExpression ?? throw new ArgumentNullException(nameof(countExpression));
    }


    public IROrderedExpressionNode ByExpression { get; }
    public IRExpressionNode CountExpression { get; }

    public override int ChildCount => 2;

    public override IRNode GetChild(int index)
    {
        return index switch
        {
            0 => CountExpression,
            1 => ByExpression,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
    {
        return visitor.VisitTopOperator(this, context);
    }

    public override string ToString()
    {
        return $"TopOperator: {SchemaDisplay.GetText(ResultType)}";
    }
}
