﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using KustoLoco.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitPrintOperator(IRPrintOperatorNode node, EvaluationContext context)
    {
        var tableSymbol = (TableSymbol)node.ResultType;

        var results = new EvaluationResult[node.Expressions.ChildCount];
        for (var i = 0; i < node.Expressions.ChildCount; i++)
        {
            var expression = node.Expressions.GetChild(i);
            results[i] = expression.Accept(this, context);
        }

        var columns = BuiltInsHelper.CreateResultArray(results)
            .Select(c => c.Column).ToArray();
        var result = new InMemoryTableSource(tableSymbol, columns);
        return TabularResult.CreateUnvisualized(result);
    }
}