// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using BabyKusto.Core.Evaluation.BuiltIns;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitCountOperator(CountOperator node)
        {
            // `T | count` is equivalent to `T | summarize count()` (except for the output column name)
            var aggregations = new List<IRExpressionNode>
            {
                new IRAggregateCallNode(
                    Aggregates.Count.Signatures[0],
                    BuiltInAggregates.GetOverload(Aggregates.Count, new IRExpressionNode[0], new List<Parameter>()),
                    new List<Parameter>(),
                    IRListNode<IRExpressionNode>.Empty,
                    ScalarTypes.Long),
            };
            return new IRSummarizeOperatorNode(aggregations: IRListNode.From(aggregations), byColumns: IRListNode<IRExpressionNode>.Empty, node.ResultType);
        }
    }
}
