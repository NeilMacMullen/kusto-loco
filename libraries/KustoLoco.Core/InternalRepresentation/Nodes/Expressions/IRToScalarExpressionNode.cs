//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRToScalarExpressionNode : IRExpressionNode
{
    // Irrespective of the result kind of the expression, the result is always a Scalar
    public IRToScalarExpressionNode(IRExpressionNode expression, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Scalar) =>
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));


    public IRExpressionNode Expression { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Expression,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitToScalarExpressionNode(this, context);

    public override string ToString() => $"ToScalarExpression: {SchemaDisplay.GetText(ResultType)}";
}


internal class IRPreEvaluatedScalarExpressionNode : IRExpressionNode
{
    // Irrespective of the result kind of the expression, the result is always a Scalar
    public IRPreEvaluatedScalarExpressionNode(object ? data, TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Scalar)
    {
        _data = data;
    }

    private readonly object? _data;

    public override int ChildCount => 0;

    public override IRNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
           (new ScalarResult(DynamicAnySymbol.From("dynamic"),_data) as TResult) !;

    public override string ToString() => $"ToScalarExpression: {SchemaDisplay.GetText(ResultType)}";
}
