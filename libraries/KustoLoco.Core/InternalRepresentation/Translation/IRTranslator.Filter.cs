// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitFilterOperator(FilterOperator node)
    {
        var irCondition = (IRExpressionNode)node.Condition.Accept(this);
        return new IRFilterOperatorNode(irCondition, node.ResultType);
    }
}