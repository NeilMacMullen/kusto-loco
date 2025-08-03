//
// Licensed under the MIT License.

using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRUnionOperatorNode : IRQueryOperatorNode
{
    public IRUnionOperatorNode(IRListNode<IRExpressionNode> expressions, TypeSymbol resultType)
        : base(resultType) =>
        Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));

    public IRListNode<IRExpressionNode> Expressions { get; }

    public override int ChildCount => 1;

    public override IRNode GetChild(int index) =>
        index switch
        {
            0 => Expressions,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
         =>
        visitor.VisitUnionOperator(this, context);

    public override string ToString() => $"UnionOperator: {SchemaDisplay.GetText(ResultType)}";
}