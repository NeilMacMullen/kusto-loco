// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitNameReference(IRNameReferenceNode node, EvaluationContext context)
        {
            var lookup = context.Scope.Lookup(node.ReferencedSymbol.Name);
            if (lookup == null)
            {
                throw new InvalidOperationException($"Name {node.ReferencedSymbol.Name} is not in scope.");
            }

            var (symbol, value) = lookup.Value;
            if (symbol != node.ReferencedSymbol)
            {
                Console.WriteLine($"Name '{node.ReferencedSymbol.Name}' mismatched, but that's expected for now in function calls.");
            }

            return value;
        }

        public override EvaluationResult? VisitRowScopeNameReferenceNode(IRRowScopeNameReferenceNode node, EvaluationContext context)
        {
            Debug.Assert(context.Chunk != null);
            var column = context.Chunk.Columns[node.ReferencedColumnIndex];
            return new ColumnarResult(column);
        }

        public override EvaluationResult? VisitLiteralExpression(IRLiteralExpressionNode node, EvaluationContext context)
        {
            return new ScalarResult(node.ResultType, node.Value);
        }

        public override EvaluationResult? VisitPipeExpression(IRPipeExpressionNode node, EvaluationContext context)
        {
            var left = (TabularResult)node.Expression.Accept(this, context);
            if (left == null)
            {
                throw new InvalidOperationException($"Left expression produced null result");
            }

            return node.Operator.Accept(this, context with { Left = left });
        }

        public override EvaluationResult? VisitDataTableExpression(IRDataTableExpression node, EvaluationContext context)
        {
            var tableSymbol = (TableSymbol)node.ResultType;

            int numColumns = tableSymbol.Columns.Count;
            int numRows = node.Data.Length / numColumns;

            var columns = new Column[numColumns];
            for (int j = 0; j < numColumns; j++)
            {
                var columnData = new object?[numRows];
                for (int i = 0; i < numRows; i++)
                {
                    columnData[i] = node.Data[i * numColumns + j];
                }

                columns[j] = ColumnHelpers.CreateFromObjectArray(columnData, tableSymbol.Columns[j].Type);
            }

            var result = new InMemoryTableSource(tableSymbol, columns);
            return new TabularResult(result);
        }
    }
}
