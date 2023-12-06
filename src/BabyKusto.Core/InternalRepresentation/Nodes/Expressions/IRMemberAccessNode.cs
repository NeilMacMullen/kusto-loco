// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRMemberAccessNode : IRExpressionNode
{
    public readonly IRExpressionNode Expression;
    public readonly string MemberName;

    public IRMemberAccessNode(IRExpressionNode expression, string memberName, TypeSymbol resultType)
        : base(resultType, expression.ResultKind)
    {
        Expression = expression;
        MemberName = memberName;
    }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitMemberAccess(this, context);

    public override string ToString() => $"IRMemberAccess({MemberName}): {SchemaDisplay.GetText(ResultType)}";
}


internal class IRArrayAccessNode : IRExpressionNode
{
    public readonly IRExpressionNode Expression;
    public readonly int Index;

    public IRArrayAccessNode(IRExpressionNode expression,int index, TypeSymbol resultType)
        : base(resultType, expression.ResultKind)
    {
        Expression = expression;
        Index = index;
    }

    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
        =>
            visitor.VisitMemberAccess(this, context);

    public override string ToString() => $"IRArrayAccess({Index}): {SchemaDisplay.GetText(ResultType)}";
}