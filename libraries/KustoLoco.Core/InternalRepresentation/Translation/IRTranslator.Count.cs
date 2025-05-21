// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitCountOperator(CountOperator node)
    {
        // `T | count` is equivalent to `T | summarize count()` (except for the output column name)
        var aggregations = new List<IRExpressionNode>
        {
            new IRAggregateCallNode(
                Aggregates.Count.Signatures[0],
                BuiltInAggregates.GetOverload(Aggregates.Count,
                TypeMapping.SymbolForType(typeof(long)),
                    [], []),
                new List<Parameter>(),
                IRListNode<IRExpressionNode>.Empty,
                ScalarTypes.Long)
        };
        return new IRSummarizeOperatorNode(IRListNode.From(aggregations),
            IRListNode<IRExpressionNode>.Empty, node.ResultType);
    }
}
