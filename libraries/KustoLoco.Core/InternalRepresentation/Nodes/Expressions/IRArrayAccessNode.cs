using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation;

internal class IRArrayAccessNode : IRExpressionNode
{
    public readonly IRExpressionNode Expression;
    public readonly int Index;

    public IRArrayAccessNode(IRExpressionNode expression, int index, TypeSymbol resultType)
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