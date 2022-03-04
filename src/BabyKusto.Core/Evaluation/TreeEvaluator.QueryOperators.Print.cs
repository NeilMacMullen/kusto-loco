// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitPrintOperator(IRPrintOperatorNode node, EvaluationContext context)
        {
            var tableSymbol = (TableSymbol)node.ResultType;

            var columns = new Column[node.Expressions.ChildCount];
            for (int i = 0; i < node.Expressions.ChildCount; i++)
            {
                var expression = node.Expressions.GetChild(i);
                var expressionResult = expression.Accept(this, context);
                Debug.Assert(expressionResult != null);
                var scalarResult = (ScalarResult)expressionResult;
                columns[i] = ColumnHelpers.CreateFromScalar(scalarResult.Value, tableSymbol.Columns[i].Type, 1);
            }

            var result = new InMemoryTableSource(tableSymbol, columns);
            return new TabularResult(result);
        }
    }
}
