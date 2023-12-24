using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitGetSchemaOperator(GetSchemaOperator node) => new IRGetSchemaOperatorNode();
}