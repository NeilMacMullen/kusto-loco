//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Diagnostics;
using KustoLoco.Core.Extensions;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;



internal partial class TreeEvaluator
{
    public override EvaluationResult VisitSummarizeOperator(IRSummarizeOperatorNode node, EvaluationContext context)
    {
        MyDebug.Assert(context.Left != TabularResult.Empty);
        var byExpressions = new List<IRExpressionNode>();
        for (var i = 0; i < node.ByColumns.ChildCount; i++) byExpressions.Add(node.ByColumns.GetTypedChild(i));

        var aggregationExpressions = new List<IRExpressionNode>();
        for (var i = 0; i < node.Aggregations.ChildCount; i++)
            aggregationExpressions.Add(node.Aggregations.GetTypedChild(i));

        //we have to go to some trouble to ensure we use the correct types since
        //the parser gets it wrong this only applies to things like
        //sumarize max(int(a)) not tuples ops like summarize arg_max
        var symbol = (TableSymbol)node.ResultType;
        
        var types = byExpressions.Concat(aggregationExpressions)
            .Select(n => n.ResultType);
        if (!types.Any(t => t is TupleSymbol))
        {
            var columns = symbol.Columns.Zip(types)
                .Select((pair) => new ColumnSymbol(pair.First.Name, pair.Second))
                .ToArray();

             symbol = new TableSymbol(symbol.Name, columns);
        }

        var result = new SummarizeResultTable(this, context.Left.Value, context, byExpressions,
            aggregationExpressions, symbol);
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
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

        protected override SummarizeResultTableContext Init()
        {
            return new SummarizeResultTableContext
            {
                BucketizedTables = new CachedDictionary<SummaryKey, SummarySet>(1)
            };
        }

        private static SummarySet GetOrAddBucket(SummaryKey key,
            SummarizeResultTableContext context)
        {
            if (context.BucketizedTables.TryGetValue(key, out var bucket)) return bucket;
            
            bucket =
                new SummarySet(key.GetArray(),
                    [], []);
            context.BucketizedTables.Add(key, bucket);

            return bucket;
        }

        protected override (SummarizeResultTableContext NewContext, ITableChunk NewChunk, bool ShouldBreak)
            ProcessChunk(SummarizeResultTableContext context, ITableChunk chunk)
        {
            var byValuesColumns = new List<BaseColumn>(_byExpressions.Count);

            var chunkContext = _context with { Chunk = chunk };
            foreach (var byExpression in _byExpressions)
            {
                var byExpressionResult = (ColumnarResult)byExpression.Accept(_owner, chunkContext);
                MyDebug.Assert(byExpressionResult.Type.Simplify() == byExpression.ResultType.Simplify(),
                    $"By expression produced wrong type {byExpressionResult.Type}, expected {byExpression.ResultType}.");
                byValuesColumns.Add(byExpressionResult.Column);
            }

            if (byValuesColumns.Any())
            {
                //it's important that we isolate the results for each chunk
                //before merging them back to the global summary context
                var thisChunkContext = new SummarizeResultTableContext
                    { BucketizedTables = new CachedDictionary<SummaryKey, SummarySet>(1) };

                for (var rowIndex = 0; rowIndex < chunk.RowCount; rowIndex++)
                {
                    //although it's tempting to use a linq select here, 
                    //this loop has to be very performant, and it's significantly
                    //faster to set properties in a for loop
                    var key = new SummaryKey();
                    for (var c = 0; c < byValuesColumns.Count; c++)
                        key.Set(c, byValuesColumns[c].GetRawDataValue(rowIndex));
                    var bucket = GetOrAddBucket(key, thisChunkContext);
                    var rowList = bucket.RowIds;
                    rowList.Add(rowIndex);
                }

                foreach (var (summaryKey, summary) in thisChunkContext.BucketizedTables)
                {
                    var wantedRowChunk = ChunkHelpers.Slice(chunk, [..summary.RowIds]);
                    //copy the values we found for this chunk back into the global context
                    var set = GetOrAddBucket(summaryKey, context);
                    set.SummarisedChunks.Add(wantedRowChunk);
                }

            }
            else
            {
                //If we are not actually summarizing then we can just return the original chunk
                var bucket = GetOrAddBucket(new SummaryKey(), context);
                bucket.SummarisedChunks.Add(chunk);
            }

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
                    var aggregationResult = aggregationExpression.Accept(_owner, chunkContext);
                    if (aggregationResult is ScalarResult scalar)
                    {
                        MyDebug.Assert(scalar.Type.Simplify() == aggregationExpression.ResultType.Simplify(),
                            $"Aggregation expression produced wrong type {SchemaDisplay.GetText(scalar.Type)}, expected {SchemaDisplay.GetText(aggregationExpression.ResultType)}.");
                        resultColumns[summarySet.ByValues.Length + i].Add(scalar.Value);
                    }

                    if (aggregationResult is RowResult rowResult)
                    {
                        var offset = 0;
                        foreach (var v in rowResult.Values)
                        {
                            resultColumns[summarySet.ByValues.Length + offset].Add(v);
                            offset++;
                        }
                    }
                }
            }

            var resultChunk = new TableChunk(this, resultColumns.Select(c => c.ToColumn()).ToArray());

            return resultChunk;
        }
    }

    private struct SummarizeResultTableContext
    {
        public CachedDictionary<SummaryKey, SummarySet> BucketizedTables;
    }
}
