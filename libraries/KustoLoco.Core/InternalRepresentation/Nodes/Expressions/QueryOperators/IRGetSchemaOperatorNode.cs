using Kusto.Language.Symbols;


namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRGetSchemaOperatorNode : IRQueryOperatorNode
{
    public IRGetSchemaOperatorNode()
        : base(ScalarTypes.Null)
    {
    }


    public override int ChildCount => 0;


    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context) =>
        visitor.VisitGetSchemaOperator(this, context);

    public override string ToString() => "GetSchemaOperator";
}
