// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    private const int MaxRowsPerChunk = 100_000;

    public override EvaluationResult VisitRangeOperator(IRRangeOperatorNode node, EvaluationContext context)
    {
        var fromExpressionResult = (ScalarResult)node.FromExpression.Accept(this, context);
        var toExpressionResult = (ScalarResult)node.ToExpression.Accept(this, context);
        var stepExpressionResult = (ScalarResult)node.StepExpression.Accept(this, context);

        var resultType = (TableSymbol)node.ResultType;

        var result = CreateTable(fromExpressionResult, toExpressionResult, stepExpressionResult, resultType);
        return TabularResult.CreateUnvisualized(result);

        // Local function
        static ITableSource CreateTable(ScalarResult from, ScalarResult to, ScalarResult step,
            TableSymbol resultType)
        {
            var columnType = resultType.Columns[0].Type!;

            if (columnType == ScalarTypes.Long)
                return new RangeResultTable<long>(from, to, step, resultType);

            if (columnType == ScalarTypes.Int)
                return new RangeResultTable<int>(from, to, step, resultType);

            if (columnType == ScalarTypes.Real)
                return new RangeResultTable<double>(from, to, step, resultType);
            if (columnType == ScalarTypes.TimeSpan)
                return new RangeResultTableForTimeSpan(from, to, step, resultType);

            if (columnType == ScalarTypes.DateTime)
                return new RangeResultTableForDateTime(from, to, step, resultType);

            throw new NotImplementedException($"Range operator not implemented for type: {columnType}");
        }
    }

    private static T GetValue<T>(object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private class RangeResultTable<T> : ITableSource
        where T : struct, INumber<T>
    {
        private readonly T _from;
        private readonly T _step;
        private readonly T _to;

        public RangeResultTable(ScalarResult from, ScalarResult to, ScalarResult step, TableSymbol resultType)
        {
            Type = resultType;
            if (from.Value is null || to.Value is null || step.Value is null)
            {
                // Return empty table
                _from = T.One;
                _to = T.Zero;
                _step = T.One;
                return;
            }

            _from = GetValue<T>(from.Value);
            _to = GetValue<T>(to.Value);
            _step = GetValue<T>(step.Value);
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            var direction = _to >= _from ? T.One : -T.One;
            var stepDirection = _step >= T.Zero ? T.One : -T.One;

            if (_step == T.Zero || direction != stepDirection)
            {
                yield return new TableChunk(this,
                    [ColumnFactory.Create(Array.Empty<T?>())]);
                yield break;
            }

            Func<T, bool> isDone = direction == T.One
                ? val => val <= _to
                : val => val >= _to;

            var chunk = new T?[MaxRowsPerChunk];
            var i = 0;
            for (var val = _from; isDone(val); val += _step)
            {
                chunk[i++] = val;

                if (i != MaxRowsPerChunk)
                    continue;

                yield return new TableChunk(this, [ColumnFactory.Create(chunk)]);
                i = 0;
            }

            if (i <= 0)
                yield break;

            // Smaller end chunk
            Array.Resize(ref chunk, i);

            yield return new TableChunk(this, [ColumnFactory.Create(chunk)]);
        }

        public IAsyncEnumerable<ITableChunk> GetDataAsync(
            CancellationToken cancellation = default)
        {
            return GetData().ToAsyncEnumerable();
        }
    }

    private class RangeResultTableForTimeSpan : ITableSource
    {
        private readonly TimeSpan _from;
        private readonly TimeSpan _step;
        private readonly TimeSpan _to;

        public RangeResultTableForTimeSpan(ScalarResult from, ScalarResult to, ScalarResult step,
            TableSymbol resultType)
        {
            Type = resultType;
            if (from.Value is null || to.Value is null || step.Value is null)
            {
                // Return empty table
                _from = TimeSpan.Zero;
                _to = TimeSpan.Zero;
                _step = TimeSpan.MaxValue;
                return;
            }

            _from = GetValue<TimeSpan>(from.Value);
            _to = GetValue<TimeSpan>(to.Value);
            _step = GetValue<TimeSpan>(step.Value);
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            var direction = _to >= _from ? 1 : -1;
            var stepDirection = _step >= TimeSpan.Zero ? 1 : -1;

            if (_step == TimeSpan.Zero || direction != stepDirection)
            {
                yield return new TableChunk(this,
                    [ColumnFactory.Create(Array.Empty<DateTime?>())]);
                yield break;
            }

            Func<TimeSpan, bool> isDone = direction == 1
                ? val => val <= _to
                : val => val >= _to;

            var chunk = new TimeSpan?[MaxRowsPerChunk];
            var i = 0;
            for (var val = _from; isDone(val); val += _step)
            {
                chunk[i++] = val;

                if (i != MaxRowsPerChunk)
                    continue;

                yield return new TableChunk(this, [ColumnFactory.Create(chunk)]);
                i = 0;
            }

            if (i <= 0)
                yield break;

            // Smaller end chunk
            Array.Resize(ref chunk, i);

            yield return new TableChunk(this, [ColumnFactory.Create(chunk)]);
        }

        public IAsyncEnumerable<ITableChunk> GetDataAsync(
            CancellationToken cancellation = default)
        {
            return GetData().ToAsyncEnumerable();
        }
    }

    private class RangeResultTableForDateTime : ITableSource
    {
        private readonly DateTime _from;
        private readonly TimeSpan _step;
        private readonly DateTime _to;

        public RangeResultTableForDateTime(ScalarResult from, ScalarResult to, ScalarResult step,
            TableSymbol resultType)
        {
            Type = resultType;
            if (from.Value is null || to.Value is null || step.Value is null)
            {
                // Return empty table
                _from = DateTime.MinValue;
                _to = DateTime.MinValue;
                _step = TimeSpan.MaxValue;
                return;
            }

            _from = GetValue<DateTime>(from.Value);
            _to = GetValue<DateTime>(to.Value);
            _step = GetValue<TimeSpan>(step.Value);
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            var direction = _to >= _from ? 1 : -1;
            var stepDirection = _step >= TimeSpan.Zero ? 1 : -1;

            if (_step == TimeSpan.Zero || direction != stepDirection)
            {
                yield return new TableChunk(this,
                    [ColumnFactory.Create(Array.Empty<DateTime?>())]);
                yield break;
            }

            Func<DateTime, bool> isDone = direction == 1
                ? val => val <= _to
                : val => val >= _to;

            var chunk = new DateTime?[MaxRowsPerChunk];
            var i = 0;
            for (var val = _from; isDone(val); val += _step)
            {
                chunk[i++] = val;

                if (i != MaxRowsPerChunk)
                    continue;

                yield return new TableChunk(this, [ColumnFactory.Create(chunk)]);
                i = 0;
            }

            if (i <= 0)
                yield break;

            // Smaller end chunk
            Array.Resize(ref chunk, i);

            yield return new TableChunk(this, [ColumnFactory.Create(chunk)]);
        }

        public IAsyncEnumerable<ITableChunk> GetDataAsync(
            CancellationToken cancellation = default)
        {
            return GetData().ToAsyncEnumerable();
        }
    }
}
