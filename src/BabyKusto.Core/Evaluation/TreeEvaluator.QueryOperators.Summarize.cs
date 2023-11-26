// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitSummarizeOperator(IRSummarizeOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != null);
        var byExpressions = new List<IRExpressionNode>();
        for (var i = 0; i < node.ByColumns.ChildCount; i++)
        {
            byExpressions.Add(node.ByColumns.GetTypedChild(i));
        }

        var aggregationExpressions = new List<IRExpressionNode>();
        for (var i = 0; i < node.Aggregations.ChildCount; i++)
        {
            aggregationExpressions.Add(node.Aggregations.GetTypedChild(i));
        }

        var result = new SummarizeResultTable(this, context.Left.Value, context, byExpressions,
            aggregationExpressions, (TableSymbol)node.ResultType);
        return new TabularResult(result, context.Left.VisualizationState);
    }

    private class SummarizeResultTable : DerivedTableSourceBase<SummarizeResultTableContext>
    {
        private readonly List<IRExpressionNode> _aggregationExpressions;
        private readonly List<IRExpressionNode> _byExpressions;
        private readonly EvaluationContext _context;
        private readonly TreeEvaluator _owner;

        public SummarizeResultTable(TreeEvaluator owner, ITableSource input, EvaluationContext context,
            List<IRExpressionNode> byExpressions, List<IRExpressionNode> aggregationExpressions,
            TableSymbol resultType)
            : base(input)
        {
            _owner = owner;
            _context = context;
            _byExpressions = byExpressions;
            _aggregationExpressions = aggregationExpressions;
            Type = resultType;
        }

        public override TableSymbol Type { get; }

        protected override SummarizeResultTableContext Init() =>
            new()
            {
                BucketizedTables = new Dictionary<string, NpmSummarySet>()
            };

        protected override (SummarizeResultTableContext NewContext, ITableChunk? NewChunk, bool ShouldBreak)
            ProcessChunk(SummarizeResultTableContext context, ITableChunk chunk)
        {
            // TODO: This is horribly inefficient
            //  * Copies all data, even columns that aren't used
            //  * Composite key calculation involves lots of string allocations and escapings
            // NPM - introduce record key type.  Also summaries could just be lists of rows referenced from
            // original table/columns
            var numInputColumns = Source.Type.Columns.Count;
            var byValuesColumns = new List<Column>(_byExpressions.Count);
            {
                var chunkContext = _context with { Chunk = chunk };
                for (var i = 0; i < _byExpressions.Count; i++)
                {
                    var byExpression = _byExpressions[i];
                    var byExpressionResult = (ColumnarResult?)byExpression.Accept(_owner, chunkContext);
                    Debug.Assert(byExpressionResult != null);
                    Debug.Assert(byExpressionResult.Type.Simplify() == byExpression.ResultType.Simplify(),
                        $"By expression produced wrong type {byExpressionResult.Type}, expected {byExpression.ResultType}.");
                    byValuesColumns.Add(byExpressionResult.Column);
                }
            }

            for (var rowIndex = 0; rowIndex < chunk.RowCount; rowIndex++)
            {
                var byValues = byValuesColumns.Select(c => c.GetRawDataValue(rowIndex)).ToList();

                // TODO: Should nulls be treated differently than empty string?
                // TODO: Use a less expensive composite key computation
                var key = string.Join("|", byValues.Select(v => Uri.EscapeDataString(v?.ToString() ?? "")));

                if (!context.BucketizedTables.TryGetValue(key, out var bucket))
                {
                    var builders = chunk.Columns.Select(col => col.CreateIndirectBuilder()).ToArray();

                    context.BucketizedTables[key] = bucket =
                        new NpmSummarySet(byValues!,
                            builders,
                            new List<int>());
                }


                bucket.SelectedRows.Add(rowIndex);
            }

            return (context, null, false);
        }

        protected override ITableChunk? ProcessLastChunk(SummarizeResultTableContext context)
        {
            var resultsData = new ColumnBuilder[_byExpressions.Count + _aggregationExpressions.Count];
            for (var i = 0; i < resultsData.Length; i++)
            {
                resultsData[i] = ColumnHelpers.CreateBuilder(Type.Columns[i].Type);
            }

            var resultRow = 0;
            foreach (var summarySet in context.BucketizedTables.Values)
            {
                for (var i = 0; i < summarySet.ByValues.Count; i++)
                {
                    resultsData[i].Add(summarySet.ByValues[i]);
                }

                var rows = summarySet.SelectedRows.ToArray();

                var bucketChunk =
                    new TableChunk(Source,
                        summarySet.IndirectionBuilders.Select(c => c.CreateIndirectColumn(rows)).ToArray());
                var chunkContext = _context with { Chunk = bucketChunk };
                for (var i = 0; i < _aggregationExpressions.Count; i++)
                {
                    var aggregationExpression = _aggregationExpressions[i];
                    var aggregationResult = (ScalarResult?)aggregationExpression.Accept(_owner, chunkContext);
                    Debug.Assert(aggregationResult != null);
                    Debug.Assert(aggregationResult.Type.Simplify() == aggregationExpression.ResultType.Simplify(),
                        $"Aggregation expression produced wrong type {SchemaDisplay.GetText(aggregationResult.Type)}, expected {SchemaDisplay.GetText(aggregationExpression.ResultType)}.");
                    resultsData[summarySet.ByValues.Count + i].Add(aggregationResult.Value);
                }

                resultRow++;
            }

            var resultChunk = new TableChunk(this, resultsData.Select(c => c.ToColumn()).ToArray());
            return resultChunk;
        }
    }

    private struct SummarizeResultTableContext
    {
        public Dictionary<string, NpmSummarySet> BucketizedTables;
    }

    private readonly record struct NpmSummarySet(List<object> ByValues,
        IndirectColumnBuilder[] IndirectionBuilders,
        List<int> SelectedRows);
}