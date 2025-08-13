//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    private partial class JoinResultTable : ITableSource
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
            var leftTable = ChunkHelpers.FromITableSource(_left);
            var rightTable = ChunkHelpers.FromITableSource(right);
            var leftBuckets = Bucketize(leftTable, true);
            var rightBuckets = Bucketize(rightTable, false);
            return _joinKind switch
            {
                IRJoinKind.InnerUnique => InnerJoin(leftTable, rightTable, leftBuckets, rightBuckets, true),
                IRJoinKind.Inner => InnerJoin(leftTable, rightTable, leftBuckets, rightBuckets, false),
                IRJoinKind.LeftOuter => LeftOuterJoin(leftTable, rightTable,leftBuckets, rightBuckets),
                IRJoinKind.RightOuter => RightOuterJoin(leftTable, rightTable,leftBuckets, rightBuckets),
                IRJoinKind.FullOuter => FullOuterJoin(leftTable, rightTable,leftBuckets, rightBuckets),
                IRJoinKind.LeftSemi => LeftSemiJoin(leftTable, rightTable,leftBuckets, rightBuckets),
                IRJoinKind.RightSemi => RightSemiJoin(leftBuckets, rightBuckets),
                IRJoinKind.LeftAnti => LeftAntiJoin(leftBuckets, rightBuckets),
                IRJoinKind.RightAnti => RightAntiJoin(leftBuckets, rightBuckets),
                _ => throw new NotImplementedException($"Join kind {_joinKind} is not supported yet.")
            };
        }

        public IAsyncEnumerable<ITableChunk> DataAsync(CancellationToken cancellation = default) =>
            throw new NotSupportedException();

        private BucketedRows Bucketize(ITableSource table, bool isLeft)
        {
            var result = new BucketedRows(table);
            var onExpressions = _onClauses.Select(c => isLeft ? c.Left : c.Right).ToArray();
            var chunkIndex = 0;
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
                        bucket = new JoinSet();
                        result.Buckets.Add(key, bucket);
                    }

                    bucket.Add(chunk, i);
                }

                chunkIndex++;
            }

            return result;
        }

        private static void AddPartialRow(BaseColumnBuilder[] builders,
            ITableChunk chunk, int row)
        {
            for (var c = 0; c < builders.Length; c++)
            {
                var data = chunk.Columns[c]
                    .GetRawDataValue(row);
                builders[c].Add(data);
            }
        }

        private static void AddPartialRow(List<int> indices, int row)
            => indices.Add(row);

        /// <param name="dedupeLeft">
        ///     When true, takes the first left match of each bucket instead of all.
        ///     In other words, setting <paramref name="dedupeLeft" /> to true produces the default join behavior (i.e.
        ///     `innerunique`).
        ///     Setting it to false produces the `inner`-join behavior.
        /// </param>
        private IEnumerable<ITableChunk> InnerJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable, BucketedRows left, BucketedRows right, bool dedupeLeft)
        {
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();
            if (_isLookup)
            {
                foreach (var rightBucket in right.Buckets)
                {
                    var rightValue = rightBucket.Value;
                    if (!left.Buckets.TryGetValue(rightBucket.Key, out var leftValue)) continue;

                    foreach (var (rightChunk, rightRow) in rightValue.Enumerate())
                    foreach (var (leftChunk, leftRow) in leftValue.Enumerate())
                    {
                        leftIndices.Add(leftRow);
                        rightIndices.Add(rightRow);
                    }
                }
                return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
            }
            //join
            foreach (var leftBucket in left.Buckets)
            {
                var leftValue = leftBucket.Value;
                var numLeftRows = dedupeLeft ? 1 : leftValue.RowCount;

                if (!right.Buckets.TryGetValue(leftBucket.Key, out var rightValue)) continue;
                var leftEnum = dedupeLeft
                    ? [leftValue.GetFirst()]
                    : leftValue.Enumerate();
                foreach (var (leftChunk, leftRow) in leftEnum)
                foreach (var (rightChunk, rightRow) in rightValue.Enumerate())
                {
                    leftIndices.Add(leftRow);
                    rightIndices.Add(rightRow);
                }
            }
            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
        }


        private ITableChunk[] CreateChunks(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable, List<int> leftIndices, List<int> rightIndices)
        {
            var leftIArray = leftIndices.ToImmutableArray();
            var rightIArray = rightIndices.ToImmutableArray();
            var leftCols = (leftTable as InMemoryTableSource)!.GetChunk().Columns
                .Select(col => ColumnHelpers.MapColumn(col, leftIArray)).ToArray();
            var rawRight = (rightTable as InMemoryTableSource)!.GetChunk().Columns;
            var rightCols = FilterRightColumnsForLookup(rawRight)
                .Select(col => ColumnHelpers.MapColumn(col, rightIArray)).ToArray();
            return ChunkFromColumns(leftCols.Concat(rightCols));
        }

        private ITableChunk[] ChunkFromBuilders(IEnumerable<BaseColumnBuilder> builders)
        {
            var columns = builders
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return [chunk];
        }

        private ITableChunk[] ChunkFromColumns(IEnumerable<BaseColumn> builders)
        {
            var columns = builders
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return [chunk];
        }


        private IEnumerable<ITableChunk> LeftSemiJoin(IMaterializedTableSource leftTable, IMaterializedTableSource rightTable, BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(left);
            foreach (var rightBucket in right.Buckets)
            {
                if (!left.Buckets.TryGetValue(rightBucket.Key, out var leftValue)) continue;

                foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    AddPartialRow(resultColumns, lChunk, lRow);
            }

            return ChunkFromBuilders(resultColumns);
        }

        private IEnumerable<ITableChunk> LeftAntiJoin(BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(left);
            foreach (var (key, leftValue) in left.Buckets)
            {
                if (right.Buckets.ContainsKey(key)) continue;
                foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    AddPartialRow(resultColumns, lChunk, lRow);
            }

            return ChunkFromBuilders(resultColumns);
        }

        private BaseColumnBuilder[] BuildersFromBucketed(BucketedRows b)
            => b.Table.Type.Columns.Select(c => ColumnHelpers.CreateBuilder(c.Type)).ToArray();


        private IEnumerable<ITableChunk> LeftOuterJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable,BucketedRows left, BucketedRows right)
        { var leftIndices = new List<int>();
            var rightIndices = new List<int>();

            //it's not really practical to optimise this for lookup
            //because we need to populate every left row
            foreach (var (key, leftValue) in left.Buckets)
                if (right.Buckets.TryGetValue(key, out var rightValue))
                    foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    foreach (var (rChunk, rRow) in rightValue.Enumerate())
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(rRow);;
                    }
                else
                    foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(-1);;
                    }

            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
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

        private BaseColumn[] FilterRightColumnsForLookup(BaseColumn[] rightColumns)
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
            {
                if (!right.Buckets.TryGetValue(leftBucket.Key, out var rightValue)) continue;
                foreach (var (rChunk, rRow) in rightValue.Enumerate())
                    AddPartialRow(resultColumns, rChunk, rRow);
            }

            return ChunkFromBuilders(resultColumns);
        }

        private IEnumerable<ITableChunk> RightAntiJoin(BucketedRows left, BucketedRows right)
        {
            var resultColumns = BuildersFromBucketed(right);

            foreach (var rightBucket in right.Buckets)
            {
                if (left.Buckets.TryGetValue(rightBucket.Key, out _)) continue;
                var rightValue = rightBucket.Value;
                foreach (var (rChunk, rRow) in rightValue.Enumerate())
                    AddPartialRow(resultColumns, rChunk, rRow);
            }

            return ChunkFromBuilders(resultColumns);
        }

        private IEnumerable<ITableChunk> RightOuterJoin(
            IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable,
            BucketedRows left, BucketedRows right)
        {
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();
            foreach (var rightBucket in right.Buckets)
            {
                var rightValue = rightBucket.Value;

                if (left.Buckets.TryGetValue(rightBucket.Key, out var leftValue))
                    foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    foreach (var (rChunk, rRow) in rightValue.Enumerate())
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(rRow);;

                    }
                else
                    foreach (var (rChunk, rRow) in rightValue.Enumerate())
                    {
                        leftIndices.Add(-1);
                        rightIndices.Add(rRow);;

                    }
            }

            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);

        }

        private IEnumerable<ITableChunk> FullOuterJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable,BucketedRows left, BucketedRows right)
        {
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();

            foreach (var leftBucket in left.Buckets)
            {
                var leftValue = leftBucket.Value;


                if (right.Buckets.TryGetValue(leftBucket.Key, out var rightValue))
                {
                    var numRightRows = rightValue.RowCount;

                    foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    foreach (var (rChunk, rRow) in rightValue.Enumerate())
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(rRow);;

                    }
                }
                else
                {
                    foreach (var (lChunk, lRow) in leftValue.Enumerate())
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(-1);;

                    }
                }
            }

            foreach (var rightBucket in right.Buckets)
            {
                var rightValue = rightBucket.Value;

                if (left.Buckets.TryGetValue(rightBucket.Key, out var leftValue)) continue;
                foreach (var (rChunk, rRow) in rightValue.Enumerate())
                {
                    leftIndices.Add(-1);
                    rightIndices.Add(rRow);;

                }
            }
            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
        }
    }
}
