// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitPrintOperator(PrintOperator node)
        {
            var irExpressions = new List<IRExpressionNode>();
            foreach (var expression in node.Expressions)
            {
                var irExpression = (IRExpressionNode)expression.Element.Accept(this);
                irExpressions.Add(irExpression);
            }

            return new IRPrintOperatorNode(IRListNode.From(irExpressions), node.ResultType);
        }
    }
}
