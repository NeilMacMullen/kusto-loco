// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
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
                return new RangeResultTableOflong(from, to, step, resultType);

            if (columnType == ScalarTypes.Int)
                return new RangeResultTableOfint(from, to, step, resultType);

            if (columnType == ScalarTypes.Real)
                return new RangeResultTableOfdouble(from, to, step, resultType);
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
                    [GenericColumnFactoryOfTimeSpan.CreateFromObjects([])]);
                yield break;
            }

            Func<TimeSpan, bool> isDone = direction == 1
                ? val => val <= _to
                : val => val >= _to;

            var chunk = NullableSetBuilderOfTimeSpan.CreateExpandable(0);
            var i = 0;
            for (var val = _from; isDone(val); val += _step)
            {
                chunk.Add(val);
                i++;

                if (i != MaxRowsPerChunk)
                    continue;

                yield return new TableChunk(this, [
                    GenericColumnFactoryOfTimeSpan.CreateFromDataSet(chunk.ToNullableSet())]);
                i = 0;
                chunk = NullableSetBuilderOfTimeSpan.CreateExpandable(0);
            }

            if (i <= 0)
                yield break;

            // Smaller end chunk

            yield return new TableChunk(this, [
                GenericColumnFactoryOfTimeSpan.CreateFromDataSet(chunk.ToNullableSet())]);
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
                    [GenericColumnFactoryOfDateTime.CreateFromObjects([])]);
                yield break;
            }

            Func<DateTime, bool> isDone = direction == 1
                ? val => val <= _to
                : val => val >= _to;

            var chunk = NullableSetBuilderOfDateTime.CreateExpandable(0);
            var i = 0;
            for (var val = _from; isDone(val); val += _step)
            {
                chunk.Add(val);
                i++;
                if (i != MaxRowsPerChunk)
                    continue;

                yield return new TableChunk(this, [
                    GenericColumnFactoryOfDateTime.CreateFromDataSet(chunk.ToNullableSet())]); ;
                i = 0;
            }

            if (i <= 0)
                yield break;

            // Smaller end chunk
           

            yield return new TableChunk(this, [
                GenericColumnFactoryOfDateTime.CreateFromDataSet(chunk.ToNullableSet())]);
        }

        public IAsyncEnumerable<ITableChunk> GetDataAsync(
            CancellationToken cancellation = default)
        {
            return GetData().ToAsyncEnumerable();
        }
    }
}
