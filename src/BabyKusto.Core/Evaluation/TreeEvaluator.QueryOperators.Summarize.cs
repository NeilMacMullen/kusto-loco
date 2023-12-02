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
using NLog;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitSummarizeOperator(IRSummarizeOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != TabularResult.Empty);
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
                BucketizedTables = new Dictionary<SummaryKey, NpmSummarySet>()
            };

        protected override (SummarizeResultTableContext NewContext, ITableChunk NewChunk, bool ShouldBreak)
            ProcessChunk(SummarizeResultTableContext context, ITableChunk chunk)
        {
            Logger.Info($"Process chunk called on chunk with {chunk.RowCount} rows");
            var byValuesColumns = new List<Column>(_byExpressions.Count);

            var chunkContext = _context with { Chunk = chunk };
            for (var i = 0; i < _byExpressions.Count; i++)
            {
                var byExpression = _byExpressions[i];
                var byExpressionResult = (ColumnarResult)byExpression.Accept(_owner, chunkContext);
                Debug.Assert(byExpressionResult.Type.Simplify() == byExpression.ResultType.Simplify(),
                    $"By expression produced wrong type {byExpressionResult.Type}, expected {byExpression.ResultType}.");
                byValuesColumns.Add(byExpressionResult.Column);
            }


            var hasAddedChunk = new Dictionary<SummaryKey, NpmSummarisedChunk>();
            if (byValuesColumns.Any())
            {
                for (var rowIndex = 0; rowIndex < chunk.RowCount; rowIndex++)
                {
                    var byValues = byValuesColumns.Select(c => c.GetRawDataValue(rowIndex)).ToArray();

                    var key = new SummaryKey(byValues);

                    if (!context.BucketizedTables.TryGetValue(key, out var bucket))
                    {
                        context.BucketizedTables[key] = bucket =
                            new NpmSummarySet(byValues!,
                                new List<NpmSummarisedChunk>());
                    }

                    if (!hasAddedChunk.TryGetValue(key, out var summarizedChunk))
                    {
                        summarizedChunk = new NpmSummarisedChunk(chunk, new List<int>());
                        hasAddedChunk[key] = summarizedChunk;
                        bucket.SummarisedChunks.Add(summarizedChunk);
                    }

                    summarizedChunk.RowIds.Add(rowIndex);
                }
            }
            else
            {
                //If we are not actually summarizing then we can just return the original chunk
                var summarizedChunk = new NpmSummarisedChunk(chunk, new List<int>());
                var key = new SummaryKey(Array.Empty<object?>());
                if (!context.BucketizedTables.TryGetValue(key, out var bucket))
                {
                    context.BucketizedTables[key] = bucket =
                        new NpmSummarySet(Array.Empty<object?>(),
                            new List<NpmSummarisedChunk>());
                }

                summarizedChunk.RowIds.AddRange(Enumerable.Range(0, chunk.RowCount));
                bucket.SummarisedChunks.Add(summarizedChunk);
            }


            return (context, TableChunk.Empty, false);
        }

        protected override ITableChunk ProcessLastChunk(SummarizeResultTableContext context)
        {
            var resultsData = new ColumnBuilder[_byExpressions.Count + _aggregationExpressions.Count];
            for (var i = 0; i < resultsData.Length; i++)
            {
                resultsData[i] = ColumnHelpers.CreateBuilder(Type.Columns[i].Type);
            }

            var resultRow = 0;
            foreach (var summarySet in context.BucketizedTables.Values)
            {
                Logger.Info("Processing summary set");
                for (var i = 0; i < summarySet.ByValues.Length; i++)
                {
                    resultsData[i].Add(summarySet.ByValues[i]);
                }

                foreach (var sumChunk in summarySet.SummarisedChunks)
                {
                    Logger.Info(
                        $"adding sum chunk of size {sumChunk.Chunk.RowCount} reduced to {sumChunk.RowIds.Count}");


                    var bucketChunk = CreateTableChunkFromSummaryChunk(sumChunk);
                    //todo - this is where we need to remerge or add another layer of indirection

                    var chunkContext = _context with { Chunk = bucketChunk };
                    for (var i = 0; i < _aggregationExpressions.Count; i++)
                    {
                        var aggregationExpression = _aggregationExpressions[i];
                        var aggregationResult = (ScalarResult)aggregationExpression.Accept(_owner, chunkContext);
                        Debug.Assert(aggregationResult.Type.Simplify() == aggregationExpression.ResultType.Simplify(),
                            $"Aggregation expression produced wrong type {SchemaDisplay.GetText(aggregationResult.Type)}, expected {SchemaDisplay.GetText(aggregationExpression.ResultType)}.");
                        resultsData[summarySet.ByValues.Length + i].Add(aggregationResult.Value);
                    }

                    resultRow++;
                }
            }

            var resultChunk = new TableChunk(this, resultsData.Select(c => c.ToColumn()).ToArray());
            return resultChunk;
        }

        private ITableChunk CreateTableChunkFromSummaryChunk(NpmSummarisedChunk sumChunk)
        {
            var rows = sumChunk.RowIds.ToArray();
            var remappedColumns =
                sumChunk.Chunk.Columns.Select(c => c.CreateIndirectBuilder(IndirectPolicy.Map)
                    .CreateIndirectColumn(rows)).ToArray();

            var bucketChunk =
                new TableChunk(sumChunk.Chunk.Table,
                    remappedColumns
                );
            return bucketChunk;
        }
    }


    private struct SummarizeResultTableContext
    {
        public Dictionary<SummaryKey, NpmSummarySet> BucketizedTables;
    }
}