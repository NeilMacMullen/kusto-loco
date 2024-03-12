using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitGetSchemaOperator(GetSchemaOperator node) => new IRGetSchemaOperatorNode();
}