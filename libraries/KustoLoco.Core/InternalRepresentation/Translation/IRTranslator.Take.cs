// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using NLog;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public override IRNode VisitTakeOperator(TakeOperator node)
    {
        var irExpression = (IRExpressionNode)node.Expression.Accept(this);
        return new IRTakeOperatorNode(irExpression, node.ResultType);
    }
}