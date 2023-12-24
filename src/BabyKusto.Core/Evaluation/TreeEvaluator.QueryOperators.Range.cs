// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using BabyKusto.Core.InternalRepresentation;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal partial class TreeEvaluator
    {
        public override EvaluationResult VisitRangeOperator(IRRangeOperatorNode node, EvaluationContext context)
        {
            var fromExpressionResult = (ScalarResult)node.FromExpression.Accept(this, context)!;
            var toExpressionResult = (ScalarResult)node.ToExpression.Accept(this, context)!;
            var stepExpressionResult = (ScalarResult)node.StepExpression.Accept(this, context)!;

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

                throw new NotImplementedException($"Range operator not implemented for type: {columnType}");
            }
        }

        private class RangeResultTable<T> : ITableSource
            where T : struct, INumber<T>
        {
            private const int MaxRowsPerChunk = 100;

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

                _from = GetValue(from.Value);
                _to = GetValue(to.Value);
                _step = GetValue(step.Value);

                static T GetValue(object value) => (T)Convert.ChangeType(value, typeof(T));
            }

            public TableSymbol Type { get; }

            public IEnumerable<ITableChunk> GetData()
            {
                var columnType = Type.Columns[0].Type;

                var direction = _to >= _from ? T.One : -T.One;
                var stepDirection = _step >= T.Zero ? T.One : -T.One;

                if (_step == T.Zero || direction != stepDirection)
                {
                    yield return new TableChunk(this,
                        new BaseColumn[] { BaseColumn.Create(columnType, Array.Empty<T?>()) });
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

                    yield return new TableChunk(this, new BaseColumn[] { BaseColumn.Create(columnType, chunk) });
                    i = 0;
                }

                if (i <= 0)
                    yield break;

                // Smaller end chunk
                Array.Resize(ref chunk, i);

                yield return new TableChunk(this, new BaseColumn[] { BaseColumn.Create(columnType, chunk) });
            }

            public IAsyncEnumerable<ITableChunk> GetDataAsync(
                CancellationToken cancellation = default)
                => GetData().ToAsyncEnumerable();
        }
    }
}