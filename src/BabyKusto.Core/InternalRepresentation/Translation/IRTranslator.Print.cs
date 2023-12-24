// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitPrintOperator(PrintOperator node)
    {
        var irExpressions = node.Expressions
            .Select(expression => (IRExpressionNode)expression.Element.Accept(this))
            .ToArray();

        return new IRPrintOperatorNode(IRListNode.From(irExpressions), node.ResultType);
    }
}