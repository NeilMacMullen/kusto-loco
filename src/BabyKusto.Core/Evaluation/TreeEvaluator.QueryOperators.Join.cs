// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitJoinOperator(IRJoinOperatorNode node, EvaluationContext context)
    {
        Debug.Assert(context.Left != null);
        var result = new JoinResultTable(this, context.Left.Value, node.Expression, node.Kind, context,
            node.OnClauses, (TableSymbol)node.ResultType);
        return new TabularResult(result, context.Left.VisualizationState);
    }

    private class JoinResultTable : ITableSource
    {
        private readonly EvaluationContext _context;
        private readonly IRJoinKind _joinKind;
        private readonly ITableSource _left;
        private readonly List<IRJoinOnClause> _onClauses;
        private readonly TreeEvaluator _owner;
        private readonly IRExpressionNode _rightExpression;

        public JoinResultTable(TreeEvaluator owner, ITableSource left, IRExpressionNode rightExpression,
            IRJoinKind joinKind, EvaluationContext context, List<IRJoinOnClause> onClauses, TableSymbol resultType)
        {
            _owner = owner;
            _left = left;
            _rightExpression = rightExpression;
            _joinKind = joinKind;
            _context = context;
            _onClauses = onClauses;
            Type = resultType;
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            var rightContext = new EvaluationContext(_context.Scope);
            var rightResult = _rightExpression.Accept(_owner, rightContext);
            if (rightResult == EvaluationResult.Null || !rightResult.IsTabular)
            {
                throw new InvalidOperationException(
                    $"Expected right expression to produce tabular result, got {SchemaDisplay.GetText(rightResult.Type)}");
            }

            var rightTabularResult = (TabularResult)rightResult;
            var right = rightTabularResult.Value;

            var leftBuckets = Bucketize(_left, isLeft: true);
            var rightBuckets = Bucketize(right, isLeft: false);

            return _joinKind switch
            {
                IRJoinKind.InnerUnique => InnerJoin(leftBuckets, rightBuckets, dedupeLeft: true),
                IRJoinKind.Inner => InnerJoin(leftBuckets, rightBuckets, dedupeLeft: false),
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

            var numColumns = table.Type.Columns.Count;
            var onExpressions = _onClauses.Select(c => isLeft ? c.Left : c.Right).ToArray();
            foreach (var chunk in table.GetData())
            {
                var onValuesColumns = new List<Column>(_onClauses.Count);
                {
                    var chunkContext = new EvaluationContext(_context.Scope, Chunk: chunk);
                    for (var i = 0; i < onExpressions.Length; i++)
                    {
                        var onExpression = onExpressions[i];
                        var onExpressionResult = (ColumnarResult?)onExpression.Accept(_owner, chunkContext);
                        Debug.Assert(onExpressionResult != null);
                        Debug.Assert(onExpressionResult.Type.Simplify() == onExpression.ResultType.Simplify(),
                            $"On expression[{i}] produced wrong type {SchemaDisplay.GetText(onExpressionResult.Type)}, expected {SchemaDisplay.GetText(onExpression.ResultType)}.");
                        onValuesColumns.Add(onExpressionResult.Column);
                    }
                }

                var numRows = chunk.RowCount;
                for (var i = 0; i < numRows; i++)
                {
                    var onValues = onValuesColumns.Select(c => c.RawData.GetValue(i)).ToList();

                    // TODO: Should nulls be treated differently than empty string?
                    // TODO: Use a less expensive composite key computation
                    var key = string.Join("|", onValues.Select(v => Uri.EscapeDataString(v?.ToString() ?? "")));
                    if (!result.Buckets.TryGetValue(key, out var bucket))
                    {
                        bucket = (OnValues: onValues, Data: new ColumnBuilder[numColumns]);
                        for (var j = 0; j < numColumns; j++)
                        {
                            bucket.Data[j] = ColumnHelpers.CreateBuilder(chunk.Columns[j].Type);
                        }

                        result.Buckets.Add(key, bucket);
                    }

                    for (var j = 0; j < numColumns; j++)
                    {
                        bucket.Data[j].Add(chunk.Columns[j].RawData.GetValue(i));
                    }
                }
            }

            return result;
        }

        /// <param name="dedupeLeft">
        ///     When true, takes the first left match of each bucket instead of all.
        ///     In other words, setting <paramref name="dedupeLeft" /> to true produces the default join behavior (i.e.
        ///     `innerunique`).
        ///     Setting it to false produces the `inner`-join behavior.
        /// </param>
        private IEnumerable<ITableChunk> InnerJoin(BucketedRows left, BucketedRows right, bool dedupeLeft)
        {
            var numLeftColumns = left.Table.Type.Columns.Count;
            var numRightColumns = right.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numLeftColumns + numRightColumns];
            for (var i = 0; i < numLeftColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(left.Table.Type.Columns[i].Type);
            }

            for (var i = 0; i < numRightColumns; i++)
            {
                resultColumns[numLeftColumns + i] = ColumnHelpers.CreateBuilder(right.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in left.Buckets)
            {
                var numLeftRows = dedupeLeft ? 1 : kvp.Value.Data[0].RowCount;
                if (right.Buckets.TryGetValue(kvp.Key, out var rightValue))
                {
                    Debug.Assert(numLeftColumns == kvp.Value.Data.Length);
                    Debug.Assert(numRightColumns == rightValue.Data.Length);
                    var numRightRows = rightValue.Data[0].RowCount;

                    for (var i = 0; i < numLeftRows; i++)
                    {
                        for (var j = 0; j < numRightRows; j++)
                        {
                            for (var c = 0; c < numLeftColumns; c++)
                            {
                                var leftCol = kvp.Value.Data[c];
                                resultColumns[c].Add(leftCol[i]);
                            }

                            for (var c = 0; c < numRightColumns; c++)
                            {
                                var rightCol = rightValue.Data[c];
                                resultColumns[numLeftColumns + c].Add(rightCol[j]);
                            }
                        }
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> LeftSemiJoin(BucketedRows left, BucketedRows right)
        {
            var numLeftColumns = left.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numLeftColumns];
            for (var i = 0; i < numLeftColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(left.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in right.Buckets)
            {
                if (left.Buckets.TryGetValue(kvp.Key, out var leftValue))
                {
                    Debug.Assert(numLeftColumns == leftValue.Data.Length);
                    for (var i = 0; i < numLeftColumns; i++)
                    {
                        resultColumns[i].AddRange(leftValue.Data[i]);
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> RightSemiJoin(BucketedRows left, BucketedRows right)
        {
            var numRightColumns = right.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numRightColumns];
            for (var i = 0; i < numRightColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(right.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in left.Buckets)
            {
                if (right.Buckets.TryGetValue(kvp.Key, out var rightValue))
                {
                    Debug.Assert(numRightColumns == rightValue.Data.Length);
                    for (var i = 0; i < numRightColumns; i++)
                    {
                        resultColumns[i].AddRange(rightValue.Data[i]);
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> LeftAntiJoin(BucketedRows left, BucketedRows right)
        {
            var numLeftColumns = left.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numLeftColumns];
            for (var i = 0; i < numLeftColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(left.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in left.Buckets)
            {
                if (!right.Buckets.ContainsKey(kvp.Key))
                {
                    Debug.Assert(numLeftColumns == kvp.Value.Data.Length);
                    for (var i = 0; i < numLeftColumns; i++)
                    {
                        resultColumns[i].AddRange(kvp.Value.Data[i]);
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> RightAntiJoin(BucketedRows left, BucketedRows right)
        {
            var numRightColumns = right.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numRightColumns];
            for (var i = 0; i < numRightColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(right.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in right.Buckets)
            {
                if (!left.Buckets.ContainsKey(kvp.Key))
                {
                    Debug.Assert(numRightColumns == kvp.Value.Data.Length);
                    for (var i = 0; i < numRightColumns; i++)
                    {
                        resultColumns[i].AddRange(kvp.Value.Data[i]);
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> LeftOuterJoin(BucketedRows left, BucketedRows right)
        {
            var numLeftColumns = left.Table.Type.Columns.Count;
            var numRightColumns = right.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numLeftColumns + numRightColumns];
            for (var i = 0; i < numLeftColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(left.Table.Type.Columns[i].Type);
            }

            for (var i = 0; i < numRightColumns; i++)
            {
                resultColumns[numLeftColumns + i] = ColumnHelpers.CreateBuilder(right.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in left.Buckets)
            {
                Debug.Assert(numLeftColumns == kvp.Value.Data.Length);
                var numLeftRows = kvp.Value.Data[0].RowCount;

                if (right.Buckets.TryGetValue(kvp.Key, out var rightValue))
                {
                    Debug.Assert(numRightColumns == rightValue.Data.Length);
                    var numRightRows = rightValue.Data[0].RowCount;

                    for (var i = 0; i < numLeftRows; i++)
                    {
                        for (var j = 0; j < numRightRows; j++)
                        {
                            for (var c = 0; c < numLeftColumns; c++)
                            {
                                var leftCol = kvp.Value.Data[c];
                                resultColumns[c].Add(leftCol[i]);
                            }

                            for (var c = 0; c < numRightColumns; c++)
                            {
                                var rightCol = rightValue.Data[c];
                                resultColumns[numLeftColumns + c].Add(rightCol[j]);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < numLeftRows; i++)
                    {
                        for (var c = 0; c < numLeftColumns; c++)
                        {
                            var leftCol = kvp.Value.Data[c];
                            resultColumns[c].Add(leftCol[i]);
                        }

                        for (var c = 0; c < numRightColumns; c++)
                        {
                            resultColumns[numLeftColumns + c].Add(null);
                        }
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> RightOuterJoin(BucketedRows left, BucketedRows right)
        {
            var numLeftColumns = left.Table.Type.Columns.Count;
            var numRightColumns = right.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numLeftColumns + numRightColumns];
            for (var i = 0; i < numLeftColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(left.Table.Type.Columns[i].Type);
            }

            for (var i = 0; i < numRightColumns; i++)
            {
                resultColumns[numLeftColumns + i] = ColumnHelpers.CreateBuilder(right.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in right.Buckets)
            {
                Debug.Assert(numRightColumns == kvp.Value.Data.Length);
                var numRightRows = kvp.Value.Data[0].RowCount;

                if (left.Buckets.TryGetValue(kvp.Key, out var leftValue))
                {
                    Debug.Assert(numLeftColumns == leftValue.Data.Length);
                    var numLeftRows = leftValue.Data[0].RowCount;

                    for (var i = 0; i < numLeftRows; i++)
                    {
                        for (var j = 0; j < numRightRows; j++)
                        {
                            for (var c = 0; c < numLeftColumns; c++)
                            {
                                var leftCol = leftValue.Data[c];
                                resultColumns[c].Add(leftCol[i]);
                            }

                            for (var c = 0; c < numRightColumns; c++)
                            {
                                var rightCol = kvp.Value.Data[c];
                                resultColumns[numLeftColumns + c].Add(rightCol[j]);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < numRightRows; i++)
                    {
                        for (var c = 0; c < numLeftColumns; c++)
                        {
                            resultColumns[c].Add(null);
                        }

                        for (var c = 0; c < numRightColumns; c++)
                        {
                            var rightCol = kvp.Value.Data[c];
                            resultColumns[numLeftColumns + c].Add(rightCol[i]);
                        }
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private IEnumerable<ITableChunk> FullOuterJoin(BucketedRows left, BucketedRows right)
        {
            var numLeftColumns = left.Table.Type.Columns.Count;
            var numRightColumns = right.Table.Type.Columns.Count;
            var resultColumns = new ColumnBuilder[numLeftColumns + numRightColumns];
            for (var i = 0; i < numLeftColumns; i++)
            {
                resultColumns[i] = ColumnHelpers.CreateBuilder(left.Table.Type.Columns[i].Type);
            }

            for (var i = 0; i < numRightColumns; i++)
            {
                resultColumns[numLeftColumns + i] = ColumnHelpers.CreateBuilder(right.Table.Type.Columns[i].Type);
            }

            foreach (var kvp in left.Buckets)
            {
                Debug.Assert(numLeftColumns == kvp.Value.Data.Length);
                var numLeftRows = kvp.Value.Data[0].RowCount;

                if (right.Buckets.TryGetValue(kvp.Key, out var rightValue))
                {
                    Debug.Assert(numRightColumns == rightValue.Data.Length);
                    var numRightRows = rightValue.Data[0].RowCount;

                    for (var i = 0; i < numLeftRows; i++)
                    {
                        for (var j = 0; j < numRightRows; j++)
                        {
                            for (var c = 0; c < numLeftColumns; c++)
                            {
                                var leftCol = kvp.Value.Data[c];
                                resultColumns[c].Add(leftCol[i]);
                            }

                            for (var c = 0; c < numRightColumns; c++)
                            {
                                var rightCol = rightValue.Data[c];
                                resultColumns[numLeftColumns + c].Add(rightCol[j]);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < numLeftRows; i++)
                    {
                        for (var c = 0; c < numLeftColumns; c++)
                        {
                            var leftCol = kvp.Value.Data[c];
                            resultColumns[c].Add(leftCol[i]);
                        }

                        for (var c = 0; c < numRightColumns; c++)
                        {
                            resultColumns[numLeftColumns + c].Add(null);
                        }
                    }
                }
            }

            foreach (var kvp in right.Buckets)
            {
                var numRightRows = kvp.Value.Data[0].RowCount;

                if (!left.Buckets.TryGetValue(kvp.Key, out var leftValue))
                {
                    for (var i = 0; i < numRightRows; i++)
                    {
                        for (var c = 0; c < numLeftColumns; c++)
                        {
                            resultColumns[c].Add(null);
                        }

                        for (var c = 0; c < numRightColumns; c++)
                        {
                            var rightCol = kvp.Value.Data[c];
                            resultColumns[numLeftColumns + c].Add(rightCol[i]);
                        }
                    }
                }
            }

            var columns = resultColumns
                .Select(c => c.ToColumn())
                .ToArray();
            var chunk = new TableChunk(this, columns);
            return new[] { chunk };
        }

        private class BucketedRows
        {
            public BucketedRows(ITableSource table) => Table = table;

            public ITableSource Table { get; }
            public Dictionary<string, (List<object?> OnValues, ColumnBuilder[] Data)> Buckets { get; } = new();
        }
    }
}