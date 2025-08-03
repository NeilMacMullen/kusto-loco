//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitJoinOperator(IRJoinOperatorNode node, EvaluationContext context)
    {
        var result = new JoinResultTable(this, context.Left.Value, node.Expression, node.Kind, context,
            node.OnClauses, (TableSymbol)node.ResultType, node.IsLookup);
        return TabularResult.CreateWithVisualisation(result, context.Left.VisualizationState);
    }

    private class JoinResultTable : ITableSource
    {
        private readonly EvaluationContext _context;
        private readonly bool _isLookup;
        private readonly IRJoinKind _joinKind;
        private readonly ITableSource _left;
        private readonly List<IRJoinOnClause> _onClauses;
        private readonly TreeEvaluator _owner;
        private readonly IRExpressionNode _rightExpression;

        public JoinResultTable(TreeEvaluator owner, ITableSource left, IRExpressionNode rightExpression,
            IRJoinKind joinKind, EvaluationContext context, List<IRJoinOnClause> onClauses, TableSymbol resultType,
            bool isLookup)
        {
            _owner = owner;
            _left = left;
            _rightExpression = rightExpression;
            _joinKind = joinKind;
            _context = context;
            _onClauses = onClauses;
            _isLookup = isLookup;
            Type = resultType;
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            var rightContext = new EvaluationContext(_context.Scope);
            var rightResult = _rightExpression.Accept(_owner, rightContext);
            if (rightResult == EvaluationResult.Null || !rightResult.IsTabular)
                throw new InvalidOperationException(
                    $"Expected right expression to produce tabular result, got {SchemaDisplay.GetText(rightResult.Type)}");

            var rightTabularResult = (TabularResult)rightResult;
            var right = rightTabularResult.Value;

            var leftBuckets = Bucketize(_left, true);
            var rightBuckets = Bucketize(right, false);
            return _joinKind switch
            {
                IRJoinKind.InnerUnique => InnerJoin(leftBuckets, rightBuckets, true),
                IRJoinKind.Inner => InnerJoin(leftBuckets, rightBuckets, false),
                IRJoinKind.LeftOuter => LeftOuterJoin(leftBuckets, rightBuckets),
                IRJoinKind.RightOuter => RightOuterJoin(leftBuckets, rightBuckets),
                IRJoinKind.FullOuter => FullOuterJoin(leftBuckets, rightBuckets),
                IRJoinKind.LeftSemi => LeftSemiJoin(leftBuckets, rightBuckets),
                IRJoinKind.RightSemi => RightSemiJoin(leftBuckets, rightBuckets),
                IRJoinKind.LeftAnti => LeftAntiJoin(leftBuckets, rightBuckets),
                IRJoinKind.RightAnti => RightAntiJoin(leftBuckets, rightBuckets),
                _ => throw new NotImplementedException($"Join kind {_joinKind} is not supported yet.")
            };
        }

        public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default) =>
            throw new NotSupportedException();

        private BucketedRows Bucketize(ITableSource table, bool isLeft)
        {
            var result = new BucketedRows(table);
            var onExpressions = _onClauses.Select(c => isLeft ? c.Left : c.Right).ToArray();
            foreach (var chunk in table.GetData())
            {
                var onValuesColumns = new List<BaseColumn>(_onClauses.Count);
                {
                    var chunkContext = new EvaluationContext(_context.Scope, chunk);
                    for (var i = 0; i < onExpressions.Length; i++)
                    {
                        var onExpression = onExpressions[i];
                        var onExpressionResult = (ColumnarResult?)onExpression.Accept(_owner, chunkContext);
                        onValuesColumns.Add(onExpressionResult!.Column);
                    }
                }
                var numRows = chunk.RowCount;
                for (var i = 0; i < numRows; i++)
                {
                    var onValues = onValuesColumns.Select(c => c.GetRawDataValue(i)).ToArray();
                    var key = new SummaryKey(onValues);
                    if (!result.Buckets.TryGetValue(key, out var bucket))
                    {
                        bucket = new NpmJoinSet(onValues, [], chunk);
                        result.Buckets.Add(key, bucket);
                    }

                    bucket.Rows.Add(i);
                }
            }

            return result;
        }

        private void AddPartialRow(BaseColumnBuilder[] builders,
            NpmJoinSet joinset, int index)
        {
            for (var c = 0; c < builders.Length; c++)
            {
                var data = joinset.Chunk.Columns[c]
                    .GetRawDataValue(joinset.Rows[index]);
                builders[c].Add(data);
            }
        }

        /// <param name="dedupeLeft">
        ///     When true, takes the first left match of each bucket instead of all.
        ///     In other words, setting <paramref name="dedupeLeft" /> to true produces the default join behavior (i.e.
        ///     `innerunique`).
        ///     Setting it to false produces the `inner`-join behavior.
        /// </param>
        private IEnumerable<ITableChunk> InnerJoin(BucketedRows left, BucketedRows right, bool dedupeLeft)
        {
            var leftColumns = BuildersFromBucketed(left);
            var rightColumns = BuildersFromBucketed(right);
            if (_isLookup)
            {
                foreach (var rightBucket in right.Buckets)
                {
                    var rightValue = rightBucket.Value;
                    var numrightRows = rightValue.RowCount;

                    if (left.Buckets.TryGetValue(rightBucket.Key, out var leftValue))
                        for (var i = 0; i < numrightRows; i++)
                        for (var j = 0; j < leftValue.RowCount; j++)
                        {
                            AddPartialRow(leftColumns, leftValue, i);
                            AddPartialRow(rightColumns, rightValue, j);
                        }
                }

                rightColumns = FilterRightColumnsForLookup(rightColumns);
            }
            else
            {
                foreach (var leftBucket in left.Buckets)
                {
                    var leftValue = leftBucket.Value;
                    var numLeftRows = dedupeLeft ? 1 : leftValue.RowCount;

                    if (right.Buckets.TryGetValue(leftBucket.Key, out var rightValue))
                        for (var i = 0; i < numLeftRows; i++)
                        for (var j = 0; j < rightValue.RowCount; j++)
                        {
                            AddPartialRow(leftColumns, leftValue, i);
                            AddPartialRow(rightColumns, rightValue, j);
                        }
                }
            }

            return ChunkFromBuilders(leftColumns.Concat(rightColumns));
        }

        private ITableChunk[] ChunkFromBuilders(IEnumerable<BaseColumnBuilder> builders)
        {
            var columns = builders
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return [chunk];
        }

        private IEnumerable<ITableChunk> LeftSemiJoin(BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(left);
            foreach (var rightBucket in right.Buckets)
                if (left.Buckets.TryGetValue(rightBucket.Key, out var leftValue))
                    for (var i = 0; i < leftValue.RowCount; i++)
                        AddPartialRow(resultColumns, leftValue, i);

            return ChunkFromBuilders(resultColumns);
        }

        private IEnumerable<ITableChunk> LeftAntiJoin(BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(left);
            foreach (var (key, leftValue) in left.Buckets)
                if (!right.Buckets.ContainsKey(key))
                    for (var i = 0; i < leftValue.RowCount; i++)
                        AddPartialRow(resultColumns, leftValue, i);


            return ChunkFromBuilders(resultColumns);
        }

        private BaseColumnBuilder[] BuildersFromBucketed(BucketedRows b)
            => b.Table.Type.Columns.Select(c => ColumnHelpers.CreateBuilder(c.Type)).ToArray();


        private IEnumerable<ITableChunk> LeftOuterJoin(BucketedRows left, BucketedRows right)
        {
            var leftColumns = BuildersFromBucketed(left);
            var rightColumns = BuildersFromBucketed(right);

            //it's not really practical to optimise this for lookup
            //because we need to populate every left row
            foreach (var (key, leftValue) in left.Buckets)
                if (right.Buckets.TryGetValue(key, out var rightValue))
                    for (var i = 0; i < leftValue.RowCount; i++)
                    for (var j = 0; j < rightValue.RowCount; j++)
                    {
                        AddPartialRow(leftColumns, leftValue, i);
                        AddPartialRow(rightColumns, rightValue, j);
                    }
                else
                    for (var i = 0; i < leftValue.RowCount; i++)
                    {
                        AddPartialRow(leftColumns, leftValue, i);
                        foreach (var t in rightColumns)
                            t.Add(null);
                    }

            rightColumns = FilterRightColumnsForLookup(rightColumns);
            return ChunkFromBuilders(leftColumns.Concat(rightColumns));
        }

        private BaseColumnBuilder[] FilterRightColumnsForLookup(BaseColumnBuilder[] rightColumns)
        {
            if (!_isLookup)
                return rightColumns;
            var rightOns = _onClauses
                .Select(c => c.Right.ReferencedColumnIndex)
                .ToArray();
            return rightColumns.Index()
                .Where(v => !rightOns.Contains(v.Index))
                .Select(v => v.Item)
                .ToArray();
        }

        private IEnumerable<ITableChunk> RightSemiJoin(BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(right);
            foreach (var leftBucket in left.Buckets)
                if (right.Buckets.TryGetValue(leftBucket.Key, out var rightValue))
                    for (var i = 0; i < rightValue.RowCount; i++)
                        AddPartialRow(resultColumns, rightValue, i);
            return ChunkFromBuilders(resultColumns);
        }

        private IEnumerable<ITableChunk> RightAntiJoin(BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(right);

            foreach (var rightBucket in right.Buckets)
                if (!left.Buckets.TryGetValue(rightBucket.Key, out _))
                {
                    var rightValue = rightBucket.Value;
                    for (var i = 0; i < rightValue.RowCount; i++)
                        AddPartialRow(resultColumns, rightBucket.Value, i);
                }

            return ChunkFromBuilders(resultColumns);
        }

        private IEnumerable<ITableChunk> RightOuterJoin(BucketedRows left, BucketedRows right)
        {
            var leftColumns = BuildersFromBucketed(left);
            var rightColumns = BuildersFromBucketed(right);
            foreach (var rightBucket in right.Buckets)
            {
                var rightValue = rightBucket.Value;

                if (left.Buckets.TryGetValue(rightBucket.Key, out var leftValue))
                    for (var i = 0; i < leftValue.RowCount; i++)
                    for (var j = 0; j < rightValue.RowCount; j++)
                    {
                        AddPartialRow(leftColumns, leftValue, i);
                        AddPartialRow(rightColumns, rightValue, j);
                    }
                else
                    for (var i = 0; i < rightValue.RowCount; i++)
                    {
                        for (var c = 0; c < leftColumns.Length; c++)
                            leftColumns[c].Add(null);
                        AddPartialRow(rightColumns, rightValue, i);
                    }
            }

            return ChunkFromBuilders(leftColumns.Concat(rightColumns));
        }

        private IEnumerable<ITableChunk> FullOuterJoin(BucketedRows left, BucketedRows right)
        {
            var leftColumns = BuildersFromBucketed(left);
            var rightColumns = BuildersFromBucketed(right);

            foreach (var leftBucket in left.Buckets)
            {
                var leftValue = leftBucket.Value;


                if (right.Buckets.TryGetValue(leftBucket.Key, out var rightValue))
                {
                    var numRightRows = rightValue.RowCount;

                    for (var i = 0; i < leftValue.RowCount; i++)
                    for (var j = 0; j < rightValue.RowCount; j++)
                    {
                        AddPartialRow(leftColumns, leftValue, i);
                        AddPartialRow(rightColumns, rightValue, j);
                    }
                }
                else
                {
                    for (var i = 0; i < leftValue.RowCount; i++)
                    {
                        AddPartialRow(leftColumns, leftValue, i);
                        for (var c = 0; c < rightColumns.Length; c++)
                            rightColumns[c].Add(null);
                    }
                }
            }

            foreach (var rightBucket in right.Buckets)
            {
                var rightValue = rightBucket.Value;

                if (!left.Buckets.TryGetValue(rightBucket.Key, out var leftValue))
                    for (var i = 0; i < rightValue.RowCount; i++)
                    {
                        for (var c = 0; c < leftColumns.Length; c++)
                            leftColumns[c].Add(null);

                        AddPartialRow(rightColumns, rightValue, i);
                    }
            }

            return ChunkFromBuilders(leftColumns.Concat(rightColumns));
        }

        private class BucketedRows
        {
            public BucketedRows(ITableSource table)
            {
                Table = table;
            }

            public ITableSource Table { get; }
            public Dictionary<SummaryKey, NpmJoinSet> Buckets { get; } = new();
        }
    }

    private readonly record struct NpmJoinSet(
        object?[] OnValues,
        List<int> Rows,
        ITableChunk Chunk)
    {
        public int RowCount => Rows.Count;
    }
}
