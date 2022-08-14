// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BabyKusto.Core.InternalRepresentation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult? VisitJoinOperator(IRJoinOperatorNode node, EvaluationContext context)
        {
            Debug.Assert(context.Left != null);
            var onExpressions = new List<IRExpressionNode>();
            for (int i = 0; i < node.OnExpressions.ChildCount; i++)
            {
                onExpressions.Add(node.OnExpressions.GetTypedChild(i));
            }

            var result = new JoinResultTable(this, context.Left.Value, node.Expression, node.Kind, context, onExpressions, (TableSymbol)node.ResultType);
            return new TabularResult(result);
        }

        private class JoinResultTable : ITableSource
        {
            private readonly TreeEvaluator _owner;
            private readonly ITableSource _left;
            private readonly IRExpressionNode _rightExpression;
            private readonly IRJoinKind _joinKind;
            private readonly EvaluationContext _context;
            private readonly List<IRExpressionNode> _onExpressions;

            public JoinResultTable(TreeEvaluator owner, ITableSource left, IRExpressionNode rightExpression, IRJoinKind joinKind, EvaluationContext context, List<IRExpressionNode> onExpressions, TableSymbol resultType)
            {
                _owner = owner;
                _left = left;
                _rightExpression = rightExpression;
                _joinKind = joinKind;
                _context = context;
                _onExpressions = onExpressions;
                Type = resultType;
            }

            public TableSymbol Type { get; }

            public IEnumerable<ITableChunk> GetData()
            {
                var rightContext = new EvaluationContext(_context.Scope);
                var rightResult = _rightExpression.Accept(_owner, rightContext);
                if (rightResult == null || !rightResult.IsTabular)
                {
                    throw new InvalidOperationException($"Expected right expression to produce tabular result, got {rightResult?.Type.Display}");
                }

                var rightTabularResult = (TabularResult)rightResult;
                var right = rightTabularResult.Value;

                var leftBuckets = Bucketize(_left);
                var rightBuckets = Bucketize(right);

                switch (_joinKind)
                {
                    case IRJoinKind.Inner:
                        return InnerJoin(leftBuckets, rightBuckets);
                    default:
                        throw new NotImplementedException($"Join kind {_joinKind} is not supported yet.");
                }
            }

            public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
            {
                throw new NotSupportedException();
            }

            private BucketedRows Bucketize(ITableSource table)
            {
                var numColumns = table.Type.Columns.Count;
                var result = new BucketedRows(numColumns);

                foreach (var chunk in table.GetData())
                {
                    var onValuesColumns = new List<Column>(_onExpressions.Count);
                    {
                        var chunkContext = new EvaluationContext(_context.Scope, Chunk: chunk);
                        for (int i = 0; i < _onExpressions.Count; i++)
                        {
                            var onExpression = _onExpressions[i];
                            var onExpressionResult = (ColumnarResult?)onExpression.Accept(_owner, chunkContext);
                            Debug.Assert(onExpressionResult != null);
                            Debug.Assert(onExpressionResult.Type == onExpression.ResultType, $"On expression[{i}] produced wrong type {onExpressionResult.Type}, expected {onExpression.ResultType}.");
                            onValuesColumns.Add(onExpressionResult.Column);
                        }
                    }

                    var numRows = chunk.RowCount;
                    for (int i = 0; i < numRows; i++)
                    {
                        var onValues = onValuesColumns.Select(c => (object?)c.RawData.GetValue(i)).ToList();

                        // TODO: Should nulls be treated differently than empty string?
                        // TODO: Use a less expensive composite key computation
                        var key = string.Join("|", onValues.Select(v => Uri.EscapeDataString(v?.ToString() ?? "")));
                        if (!result.Buckets.TryGetValue(key, out var bucket))
                        {
                            bucket = (OnValues: onValues, Data: new ColumnBuilder[numColumns]);
                            for (int j = 0; j < numColumns; j++)
                            {
                                bucket.Data[j] = ColumnHelpers.CreateBuilder(chunk.Columns[j].Type);
                            }

                            result.Buckets.Add(key, bucket);
                        }

                        for (int j = 0; j < numColumns; j++)
                        {
                            bucket.Data[j].Add(chunk.Columns[j].RawData.GetValue(i));
                        }
                    }
                }

                return result;
            }

            private IEnumerable<ITableChunk> InnerJoin(BucketedRows leftBuckets, BucketedRows rightBuckets)
            {
                var resultColumns = new ColumnBuilder[leftBuckets.NumColumns + rightBuckets.NumColumns];

                foreach (var kvp in leftBuckets.Buckets)
                {
                    if (rightBuckets.Buckets.TryGetValue(kvp.Key, out var rightValue))
                    {
                        Debug.Assert(leftBuckets.NumColumns == kvp.Value.Data.Length);
                        Debug.Assert(rightBuckets.NumColumns == rightValue.Data.Length);

                        for (int i = 0; i < kvp.Value.Data.Length; i++)
                        {
                            var col = kvp.Value.Data[i];
                            if (resultColumns[i] == null)
                            {
                                resultColumns[i] = col.Clone();
                            }
                            else
                            {
                                resultColumns[i].AddRange(col);
                            }
                        }

                        for (int i = 0; i < rightValue.Data.Length; i++)
                        {
                            var col = rightValue.Data[i];
                            if (resultColumns[i + leftBuckets.NumColumns] == null)
                            {
                                resultColumns[i + leftBuckets.NumColumns] = col.Clone();
                            }
                            else
                            {
                                resultColumns[i + leftBuckets.NumColumns].AddRange(col);
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
                public BucketedRows(int numColumns)
                {
                    NumColumns = numColumns;
                }

                public int NumColumns { get; }
                public Dictionary<string, (List<object?> OnValues, ColumnBuilder[] Data)> Buckets { get; } = new();
            }
        }
    }
}
