// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitSummarizeOperator(IRSummarizeOperatorNode node, EvaluationContext context)
        {
            Debug.Assert(context.Left != null);
            var byExpressions = new List<IRExpressionNode>();
            for (int i = 0; i < node.ByColumns.ChildCount; i++)
            {
                byExpressions.Add(node.ByColumns.GetChild(i));
            }

            var aggregationExpressions = new List<IRExpressionNode>();
            for (int i = 0; i < node.Aggregations.ChildCount; i++)
            {
                aggregationExpressions.Add(node.Aggregations.GetChild(i));
            }

            var result = new SummarizeResultTable(this, context.Left.Value, context, byExpressions, aggregationExpressions, (TableSymbol)node.ResultType);
            return new TabularResult(result);
        }

        private class SummarizeResultTable : DerivedTableSourceBase<SummarizeResultTableContext>
        {
            private readonly TreeEvaluator _owner;
            private readonly EvaluationContext _context;
            private readonly List<IRExpressionNode> _byExpressions;
            private readonly List<IRExpressionNode> _aggregationExpressions;

            public SummarizeResultTable(TreeEvaluator owner, ITableSource input, EvaluationContext context, List<IRExpressionNode> byExpressions, List<IRExpressionNode> aggregationExpressions, TableSymbol resultType)
                : base(input)
            {
                _owner = owner;
                _context = context;
                _byExpressions = byExpressions;
                _aggregationExpressions = aggregationExpressions;
                Type = resultType;
            }

            public override TableSymbol Type { get; }

            protected override SummarizeResultTableContext Init()
            {
                return new SummarizeResultTableContext
                {
                    BucketizedTables = new Dictionary<string, (List<object?> ByValues, ColumnBuilder[] OriginalData)>()
                };
            }

            protected override (SummarizeResultTableContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunk(SummarizeResultTableContext context, ITableChunk chunk)
            {
                // TODO: This is horribly inefficient
                //  * Copies all data, even columns that aren't used
                //  * Composite key calculation involves lots of string allocations and escapings

                int numInputColumns = Source.Type.Columns.Count;
                var byValuesColumns = new List<ColumnarResult>(_byExpressions.Count);
                {
                    var chunkContext = _context with { Chunk = chunk };
                    for (int i = 0; i < _byExpressions.Count; i++)
                    {
                        var byExpression = _byExpressions[i];
                        byValuesColumns.Add((ColumnarResult)byExpression.Accept(_owner, chunkContext));
                    }
                }

                for (int i = 0; i < chunk.RowCount; i++)
                {
                    var byValues = byValuesColumns.Select(c => c.Column.RawData.GetValue(i)).ToList();

                    // TODO: Should nulls be treated differently than empty string?
                    // TODO: Use a less expensive composite key computation
                    var key = string.Join("|", byValues.Select(v => Uri.EscapeDataString(v?.ToString() ?? "")));

                    var newData = new object[1];

                    if (!context.BucketizedTables.TryGetValue(key, out var bucket))
                    {
                        context.BucketizedTables[key] = bucket = (byValues, new ColumnBuilder[numInputColumns]);
                        for (int j = 0; j < numInputColumns; j++)
                        {
                            bucket.OriginalData[j] = chunk.Columns[j].CreateBuilder();
                        }
                    }

                    for (int j = 0; j < numInputColumns; j++)
                    {
                        bucket.OriginalData[j].Add(chunk.Columns[j].RawData.GetValue(i));
                    }
                }

                return (context, null, false);
            }

            protected override ITableChunk? ProcessLastChunk(SummarizeResultTableContext context)
            {
                var resultsData = new ColumnBuilder[_byExpressions.Count + _aggregationExpressions.Count];
                for (int i = 0; i < resultsData.Length; i++)
                {
                    resultsData[i] = ColumnHelpers.CreateBuilder(Type.Columns[i].Type);
                }

                int resultRow = 0;
                foreach (var tableData in context.BucketizedTables.Values)
                {
                    for (int i = 0; i < tableData.ByValues.Count; i++)
                    {
                        resultsData[i].Add(tableData.ByValues[i]);
                    }

                    var bucketChunk = new TableChunk(Source, tableData.OriginalData.Select(c => c.ToColumn()).ToArray());
                    var chunkContext = _context with { Chunk = bucketChunk };
                    for (int i = 0; i < _aggregationExpressions.Count; i++)
                    {
                        var aggregationExpression = _aggregationExpressions[i];
                        var aggregation = (ScalarResult)aggregationExpression.Accept(_owner, chunkContext);
                        resultsData[tableData.ByValues.Count + i].Add(aggregation.Value);
                    }

                    resultRow++;
                }

                var resultChunk = new TableChunk(this, resultsData.Select(c => c.ToColumn()).ToArray());
                return resultChunk;
            }
        }

        private struct SummarizeResultTableContext
        {
            public Dictionary<string, (List<object?> ByValues, ColumnBuilder[] OriginalData)> BucketizedTables;
        }
    }
}
