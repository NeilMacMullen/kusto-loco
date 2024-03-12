// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using KustoLoco.Core.Extensions;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using NLog;

namespace KustoLoco.Core.Evaluation;

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
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
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
                BucketizedTables = new Dictionary<SummaryKey, SummarySet>()
            };

        private static SummarySet GetOrAddBucket(SummaryKey key,
            SummarizeResultTableContext context)
        {
            if (!context.BucketizedTables.TryGetValue(key, out var bucket))
                context.BucketizedTables[key] = bucket =
                    new SummarySet(key.GetArray(),
                        new List<ITableChunk>(), new List<int>());

            return bucket;
        }

        protected override (SummarizeResultTableContext NewContext, ITableChunk NewChunk, bool ShouldBreak)
            ProcessChunk(SummarizeResultTableContext context, ITableChunk chunk)
        {
            //Logger.Info($"Process chunk called on chunk with {chunk.RowCount} rows");
            var byValuesColumns = new List<BaseColumn>(_byExpressions.Count);

            var chunkContext = _context with { Chunk = chunk };
            foreach (var byExpression in _byExpressions)
            {
                var byExpressionResult = (ColumnarResult)byExpression.Accept(_owner, chunkContext);
                Debug.Assert(byExpressionResult.Type.Simplify() == byExpression.ResultType.Simplify(),
                    $"By expression produced wrong type {byExpressionResult.Type}, expected {byExpression.ResultType}.");
                byValuesColumns.Add(byExpressionResult.Column);
            }

            //Console.WriteLine($"created by col {watch.ElapsedMilliseconds}ms");

            if (byValuesColumns.Any())
            {
                for (var rowIndex = 0; rowIndex < chunk.RowCount; rowIndex++)
                {
                    //although it's tempting to use a linq select here, 
                    //this loop has to be very performant and it's significantly
                    //faster to set properties in a for loop
                    var key = new SummaryKey();
                    for (var c = 0; c < byValuesColumns.Count; c++)
                        key.Set(c, byValuesColumns[c].GetRawDataValue(rowIndex));


                    var bucket = GetOrAddBucket(key, context);
                    var rowList = bucket.RowIds;
                    rowList.Add(rowIndex);
                }

                foreach (var (summaryKey, summary) in context.BucketizedTables)
                {
                    var wantedRowChunk = ChunkHelpers.Slice(chunk, summary.RowIds.ToImmutableArray());
                    var set = context.BucketizedTables[summaryKey];
                    set.SummarisedChunks.Add(wantedRowChunk);
                }
            }
            else
            {
                //If we are not actually summarizing then we can just return the original chunk
                var bucket = GetOrAddBucket(new SummaryKey(), context);
                bucket.SummarisedChunks.Add(chunk);
            }

            //Console.WriteLine($"Summarize chunk took {watch.ElapsedMilliseconds}ms");
            return (context, TableChunk.Empty, false);
        }

        protected override ITableChunk ProcessLastChunk(SummarizeResultTableContext context)
        {
            var resultColumns = ColumnHelpers.CreateBuildersForTable(Type);

            foreach (var summarySet in context.BucketizedTables.Values)
            {
                // populate the initial summary indices 
                for (var i = 0; i < summarySet.ByValues.Length; i++)
                    resultColumns[i].Add(summarySet.ByValues[i]);

                //now merge the chunks for this bucket before running any aggregation function...

                var chunksInThisBucket = summarySet.SummarisedChunks.ToArray();
                var bucketChunk = ChunkHelpers.Reassemble(chunksInThisBucket);


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
        public Dictionary<SummaryKey, SummarySet> BucketizedTables;
    }
}