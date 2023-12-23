// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core.Evaluation;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitRangeOperator(RangeOperator node)
        {
            var irFromExpression = (IRExpressionNode)node.From.Accept(this);
            var irToExpression = (IRExpressionNode)node.To.Accept(this);
            var irStepExpression = (IRExpressionNode)node.Step.Accept(this);
            var columnSymbol = node.Name.ReferencedSymbol ?? NullTypeSymbol.Instance;
            return new IRRangeOperatorNode(columnSymbol, irFromExpression, irToExpression, irStepExpression, node.ResultType);
        }
    }
}
