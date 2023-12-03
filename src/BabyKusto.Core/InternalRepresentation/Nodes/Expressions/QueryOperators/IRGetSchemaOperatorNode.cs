using Kusto.Language.Symbols;
using NLog;

namespace BabyKusto.Core.InternalRepresentation;

internal class IRGetSchemaOperatorNode : IRQueryOperatorNode
{
    private static Logger Logger = LogManager.GetCurrentClassLogger();
    public IRGetSchemaOperatorNode()
        : base(ScalarTypes.Null)
    {
        Logger.Info("GetSchemanode found");
    }

  
    public override int ChildCount => 0;

   
    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context)
    {
        Logger.Info("Getschema mode visited");
        return visitor.VisitGetSchemaOperator(this, context);
    }

    public override string ToString() => "GetSchemaOperator";
}