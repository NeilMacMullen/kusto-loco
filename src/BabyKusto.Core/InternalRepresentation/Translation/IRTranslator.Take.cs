// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitTakeOperator(TakeOperator node)
        {
            var irExpression = (IRExpressionNode)node.Expression.Accept(this);
            return new IRTakeOperatorNode(irExpression, node.ResultType);
        }
    }
}
