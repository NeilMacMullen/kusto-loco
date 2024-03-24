using System;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions;

internal class IRExpressionCoupleNode : IRNode
{
    public readonly IRExpressionNode _left;
    public readonly IRExpressionNode _right;

    public IRExpressionCoupleNode(IRExpressionNode left, IRExpressionNode right)
    {
        _left = left;
        _right = right;
    }

    public override int ChildCount => 2;
    public override IRNode GetChild(int index) => index == 0 ? _left : _right;

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        => throw new NotImplementedException();
}