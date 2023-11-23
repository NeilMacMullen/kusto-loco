// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitFilterOperator(FilterOperator node)
    {
        var irCondition = (IRExpressionNode)node.Condition.Accept(this);
        return new IRFilterOperatorNode(irCondition, node.ResultType);
    }
}