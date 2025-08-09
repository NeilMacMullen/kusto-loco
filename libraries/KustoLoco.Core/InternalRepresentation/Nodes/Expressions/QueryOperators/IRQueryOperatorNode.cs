//
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal abstract class IRQueryOperatorNode : IRExpressionNode
{
    protected IRQueryOperatorNode(TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Table)
    {
    }
}