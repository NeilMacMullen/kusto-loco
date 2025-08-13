//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            return _joinKind switch
            {
                IRJoinKind.InnerUnique => InnerJoin(leftTable, rightTable, true),
                IRJoinKind.Inner => InnerJoin(leftTable, rightTable, false),
                IRJoinKind.LeftOuter => LeftOuterJoin(leftTable, rightTable),
                IRJoinKind.RightOuter => RightOuterJoin(leftTable, rightTable),
                IRJoinKind.FullOuter => FullOuterJoin(leftTable, rightTable),
                IRJoinKind.LeftSemi => LeftSemiJoin(leftTable, rightTable),
                IRJoinKind.RightSemi => RightSemiJoin(leftTable, rightTable),
                IRJoinKind.LeftAnti => LeftAntiJoin(leftTable, rightTable),
                IRJoinKind.RightAnti => RightAntiJoin(leftTable, rightTable),
                _ => throw new NotImplementedException($"Join kind {_joinKind} is not supported yet.")
            };
        }

        private BucketedRows Bucketize(ITableSource table, bool isLeft)
        {
            var result = new BucketedRows(table);
            var onExpressions = _onClauses.Select(c => isLeft ? c.Left : c.Right).ToArray();
            foreach (var chunk in table.GetData())
            {
                var onValuesColumns = new List<BaseColumn>(_onClauses.Count);
                {
                    var chunkContext = new EvaluationContext(_context.Scope, chunk);
                    foreach (var onExpression in onExpressions)
                    {
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

                    bucket.Add(i);
                }
            }

            return result;
        }

        /// When dedupeleft is true, takes the first left match of each bucket instead of all.
        /// In other words, setting
        /// <paramref name="dedupeLeft" />
        /// to true produces the default join behavior (i.e.
        /// `innerunique`).
        /// Setting it to false produces the `inner`-join behavior.
        private IEnumerable<ITableChunk> InnerJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable, bool dedupeLeft)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();
            if (_isLookup)
            {
                foreach (var rightBucket in right.Buckets)
                {
                    var rightValue = rightBucket.Value;
                    if (!left.Buckets.TryGetValue(rightBucket.Key, out var leftValue)) continue;

                    foreach (var rightRow in rightValue.RowNumbers)
                    foreach (var leftRow in leftValue.RowNumbers)
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

                if (!right.Buckets.TryGetValue(leftBucket.Key, out var rightValue)) continue;
                var leftRows = dedupeLeft
                    ? leftValue.RowNumbers.Take(1).ToList()
                    : leftValue.RowNumbers;
                foreach (var leftRow in leftRows)
                foreach (var rightRow in rightValue.RowNumbers)
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
            var leftCols = leftTable.Chunks[0].Columns
                .Select(col => ColumnHelpers.MapColumn(col, leftIArray)).ToArray();
            var rawRight = rightTable.Chunks[0].Columns;
            var rightCols = FilterRightColumnsForLookup(rawRight)
                .Select(col => ColumnHelpers.MapColumn(col, rightIArray)).ToArray();
            return ChunkFromColumns(leftCols.Concat(rightCols));
        }

        private ITableChunk[] CreateChunks(IMaterializedTableSource table, List<int> indices)
        {
            var indicesIArray = indices.ToImmutableArray();
            var cols = table.Chunks[0].Columns
                .Select(col => ColumnHelpers.MapColumn(col, indicesIArray)).ToArray();
            return ChunkFromColumns(cols);
        }


        private ITableChunk[] ChunkFromColumns(IEnumerable<BaseColumn> builders)
        {
            var columns = builders
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return [chunk];
        }


        private IEnumerable<ITableChunk> LeftSemiJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var indices = new List<int>();
            foreach (var rightBucket in right.Buckets)
            {
                if (!left.Buckets.TryGetValue(rightBucket.Key, out var leftValue)) continue;

                foreach (var lRow in leftValue.RowNumbers)
                    indices.Add(lRow);
            }

            return CreateChunks(leftTable, indices);
        }

        private IEnumerable<ITableChunk> LeftAntiJoin(
            IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var indices = new List<int>();
            foreach (var (key, leftValue) in left.Buckets)
            {
                if (right.Buckets.ContainsKey(key)) continue;
                foreach (var lRow in leftValue.RowNumbers)
                    indices.Add(lRow);
            }

            return CreateChunks(leftTable, indices);
        }


        private IEnumerable<ITableChunk> LeftOuterJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();

            //it's not really practical to optimise this for lookup
            //because we need to populate every left row
            foreach (var (key, leftValue) in left.Buckets)
                if (right.Buckets.TryGetValue(key, out var rightValue))
                    foreach (var lRow in leftValue.RowNumbers)
                    foreach (var rRow in rightValue.RowNumbers)
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(rRow);
                    }
                else
                    foreach (var lRow in leftValue.RowNumbers)
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(-1);
                    }

            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
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

        private IEnumerable<ITableChunk> RightSemiJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var indices = new List<int>();
            foreach (var leftBucket in left.Buckets)
            {
                if (!right.Buckets.TryGetValue(leftBucket.Key, out var rightValue)) continue;
                foreach (var rRow in rightValue.RowNumbers)
                    indices.Add(rRow);
            }

            return CreateChunks(rightTable, indices);
        }

        private IEnumerable<ITableChunk> RightAntiJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var indices = new List<int>();
            foreach (var rightBucket in right.Buckets)
            {
                if (left.Buckets.TryGetValue(rightBucket.Key, out _)) continue;
                var rightValue = rightBucket.Value;
                foreach (var rRow in rightValue.RowNumbers)
                    indices.Add(rRow);
            }

            return CreateChunks(rightTable, indices);
        }

        private IEnumerable<ITableChunk> RightOuterJoin(
            IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();
            foreach (var rightBucket in right.Buckets)
            {
                var rightValue = rightBucket.Value;

                if (left.Buckets.TryGetValue(rightBucket.Key, out var leftValue))
                    foreach (var lRow in leftValue.RowNumbers)
                    foreach (var rRow in rightValue.RowNumbers)
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(rRow);
                    }
                else
                    foreach (var rRow in rightValue.RowNumbers)
                    {
                        leftIndices.Add(-1);
                        rightIndices.Add(rRow);
                    }
            }

            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
        }

        private IEnumerable<ITableChunk> FullOuterJoin(IMaterializedTableSource leftTable,
            IMaterializedTableSource rightTable)
        {
            var left = Bucketize(leftTable, true);
            var right = Bucketize(rightTable, false);
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();

            foreach (var leftBucket in left.Buckets)
            {
                var leftValue = leftBucket.Value;


                if (right.Buckets.TryGetValue(leftBucket.Key, out var rightValue))
                {
                    foreach (var lRow in leftValue.RowNumbers)
                    foreach (var rRow in rightValue.RowNumbers)
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(rRow);
                    }
                }
                else
                {
                    foreach (var lRow in leftValue.RowNumbers)
                    {
                        leftIndices.Add(lRow);
                        rightIndices.Add(-1);
                    }
                }
            }

            foreach (var rightBucket in right.Buckets)
            {
                var rightValue = rightBucket.Value;

                if (left.Buckets.TryGetValue(rightBucket.Key, out _)) continue;
                foreach (var rRow in rightValue.RowNumbers)
                {
                    leftIndices.Add(-1);
                    rightIndices.Add(rRow);
                }
            }

            return CreateChunks(leftTable, rightTable, leftIndices, rightIndices);
        }
    }
}
