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
        for (var i = 0; i < node.ByColumns.ChildCount; i++) byExpressions.Add(node.ByColumns.GetTypedChild(i));

        var aggregationExpressions = new List<IRExpressionNode>();
        for (var i = 0; i < node.Aggregations.ChildCount; i++)
            aggregationExpressions.Add(node.Aggregations.GetTypedChild(i));

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

        protected override SummarizeResultTableContext Init()
        {
            return new SummarizeResultTableContext
            {
                BucketizedTables = new Dictionary<SummaryKey, NpmSummarySet>()
            };
        }

        private static (SummaryKey key,NpmSummarySet set) GetOrAddBucket(object?[] byValues, SummarizeResultTableContext context)
        {
            var key = new SummaryKey(byValues);
            if (!context.BucketizedTables.TryGetValue(key, out var bucket))
            {
                context.BucketizedTables[key] = bucket =
                    new NpmSummarySet(byValues!,
                        new List<ITableChunk>());
            }

            return (key,bucket);
        }

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


            if (byValuesColumns.Any())
            {
                var rowListsForPartitions = new Dictionary<SummaryKey, List<int>>();
                for (var rowIndex = 0; rowIndex < chunk.RowCount; rowIndex++)
                {
                    var byValues = byValuesColumns.Select(c => c.GetRawDataValue(rowIndex)).ToArray();

                    var (key,bucket) = GetOrAddBucket(byValues,context);
                   
                    if (!rowListsForPartitions.TryGetValue(key, out var rowList))
                        rowListsForPartitions[key] = rowList = new List<int>();

                    rowList.Add(rowIndex);
                }

                foreach (var (summaryKey, rowIds) in rowListsForPartitions)
                {
                    var wantedRowChunk = ChunkHelpers.Slice(chunk, rowIds.ToArray());
                    var set = context.BucketizedTables[summaryKey];
                    set.SummarisedChunks.Add(wantedRowChunk);
                }
            }
            else
            {
                //If we are not actually summarizing then we can just return the original chunk
                var emptyByValues = Array.Empty<object?>();
                var (key,bucket) = GetOrAddBucket(emptyByValues,context);
                bucket.SummarisedChunks.Add(chunk);
            }

            return (context, TableChunk.Empty, false);
        }

        protected override ITableChunk ProcessLastChunk(SummarizeResultTableContext context)
        {
            var resultColumns = ColumnHelpers.CreateBuildersForTable(Type);

            foreach (var summarySet in context.BucketizedTables.Values)
            {
                Logger.Info("Processing summary set");
                // populate the initial summary indices 
                for (var i = 0; i < summarySet.ByValues.Length; i++) resultColumns[i].Add(summarySet.ByValues[i]);

                //now merge the chunks for this bucket before running any aggregation function...

                var chunksInThisBucket = summarySet.SummarisedChunks.ToArray();
                var bucketChunk = ChunkHelpers.Reassemble(chunksInThisBucket);
                Logger.Info(
                    $"adding sum chunk of size {bucketChunk.RowCount} ");

                var chunkContext = _context with { Chunk = bucketChunk };
                for (var i = 0; i < _aggregationExpressions.Count; i++)
                {
                    var aggregationExpression = _aggregationExpressions[i];
                    var aggregationResult = (ScalarResult)aggregationExpression.Accept(_owner, chunkContext);
                    Debug.Assert(aggregationResult.Type.Simplify() == aggregationExpression.ResultType.Simplify(),
                        $"Aggregation expression produced wrong type {SchemaDisplay.GetText(aggregationResult.Type)}, expected {SchemaDisplay.GetText(aggregationExpression.ResultType)}.");
                    resultColumns[summarySet.ByValues.Length + i].Add(aggregationResult.Value);
                }
            }

            var resultChunk = new TableChunk(this, resultColumns.Select(c => c.ToColumn()).ToArray());
            return resultChunk;
        }
    }

    private struct SummarizeResultTableContext
    {
        public Dictionary<SummaryKey, NpmSummarySet> BucketizedTables;
    }
}