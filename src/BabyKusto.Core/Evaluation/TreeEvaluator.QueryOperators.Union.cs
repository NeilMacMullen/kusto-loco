// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult VisitUnionOperator(IRUnionOperatorNode node, EvaluationContext context)
        {
            var tables = new List<ITableSource>((context.Left != null ? 1 : 0) + node.Expressions.ChildCount);
            VisualizationState? visualizationState = null;
            if (context.Left != null)
            {
                tables.Add(context.Left.Value);
                visualizationState = context.Left.VisualizationState;
            }

            for (var i = 0; i < node.Expressions.ChildCount; i++)
            {
                var expression = node.Expressions.GetChild(i);
                var expressionResult = expression.Accept(this, context);
                Debug.Assert(expressionResult != null);
                var tableResult = (TabularResult)expressionResult;
                tables.Add(tableResult.Value);

                if (tableResult.VisualizationState != null)
                {
                    visualizationState = tableResult.VisualizationState;
                }
            }

            var result = new UnionResultTable(tables, (TableSymbol)node.ResultType);
            return new TabularResult(result, visualizationState);
        }

        private class UnionResultTable : ITableSource
        {
            private readonly List<ITableSource> _tables;
            private readonly Dictionary<ColumnSymbol, int> _columnMappings;

            public UnionResultTable(List<ITableSource> tables, TableSymbol resultType)
            {
                _tables = tables;
                Type = resultType;

                _columnMappings = new();
                var i = 0;
                foreach (ColumnSymbol columnSymbol in resultType.Members)
                {
                    _columnMappings.TryAdd(columnSymbol, i);
                    foreach (var originalColumn in columnSymbol.OriginalColumns)
                    {
                        _columnMappings.TryAdd(originalColumn, i);
                    }

                    i++;
                }
            }

            public TableSymbol Type { get; }

            public IEnumerable<ITableChunk> GetData()
            {
                foreach (var table in _tables)
                {
                    foreach (var chunk in table.GetData())
                    {
                        yield return ProcessChunk(table, chunk);
                    }
                }
            }

            public async IAsyncEnumerable<ITableChunk> GetDataAsync([EnumeratorCancellation] CancellationToken cancellation)
            {
                foreach (var table in _tables)
                {
                    await foreach (var chunk in table.GetDataAsync(cancellation))
                    {
                        yield return ProcessChunk(table, chunk);
                    }
                }
            }

            private TableChunk ProcessChunk(ITableSource table, ITableChunk chunk)
            {
                var columns = new Column[Type.Columns.Count];
                for (var i = 0; i < chunk.Columns.Length; i++)
                {
                    var symbol = (ColumnSymbol)table.Type.Members[i];
                    int destinationColumnIndex;
                    if (!_columnMappings.TryGetValue(symbol, out destinationColumnIndex))
                    {
                        throw new InvalidOperationException($"Couldn't find source column to match to output column {SchemaDisplay.GetText(symbol)}");
                    }

                    columns[destinationColumnIndex] = chunk.Columns[i];
                }

                var rowCount = chunk.RowCount;
                for (var i = 0; i < columns.Length; i++)
                {
                    if (columns[i] == null)
                    {
                        columns[i] = ColumnHelpers.CreateFromScalar(null, Type.Columns[i].Type, rowCount);
                    }
                }

                return new TableChunk(this, columns);
            }
        }
    }
}
