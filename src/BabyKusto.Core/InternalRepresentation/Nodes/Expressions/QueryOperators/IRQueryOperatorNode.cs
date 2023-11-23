// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.InternalRepresentation;

internal abstract class IRQueryOperatorNode : IRExpressionNode
{
    protected IRQueryOperatorNode(TypeSymbol resultType)
        : base(resultType, EvaluatedExpressionKind.Table)
    {
    }
}