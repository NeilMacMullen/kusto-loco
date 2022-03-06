// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class RowCumSumIntFunctionImpl : IWindowFunctionImpl
    {
        public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments, ColumnarResult? previousWindowResult)
        {
            Debug.Assert(thisWindowArguments.Length == 2);
            var termCol = (Column<int?>)thisWindowArguments[0].Column;
            var restartCol = (Column<bool?>)thisWindowArguments[1].Column;

            int accumulator = 0;
            if (previousWindowResult != null)
            {
                var previousColumn = (Column<int?>)previousWindowResult.Column;
                Debug.Assert(previousColumn.RowCount > 0);

                var lastValue = previousColumn[previousColumn.RowCount - 1];
                Debug.Assert(lastValue.HasValue);

                accumulator = lastValue.Value;
            }

            var data = new int?[termCol.RowCount];
            for (int i = 0; i < termCol.RowCount; i++)
            {
                var restart = restartCol[i];
                if (restart == true)
                {
                    accumulator = 0;
                }

                var term = termCol[i];
                if (term.HasValue)
                {
                    accumulator += term.Value;
                }

                data[i] = accumulator;
            }

            return new ColumnarResult(new Column<int?>(ScalarTypes.Int, data));
        }
    }

    internal class RowCumSumLongFunctionImpl : IWindowFunctionImpl
    {
        public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments, ColumnarResult? previousWindowResult)
        {
            Debug.Assert(thisWindowArguments.Length == 2);
            var termCol = (Column<long?>)thisWindowArguments[0].Column;
            var restartCol = (Column<bool?>)thisWindowArguments[1].Column;

            long accumulator = 0;
            if (previousWindowResult != null)
            {
                var previousColumn = (Column<long?>)previousWindowResult.Column;
                Debug.Assert(previousColumn.RowCount > 0);

                var lastValue = previousColumn[previousColumn.RowCount - 1];
                Debug.Assert(lastValue.HasValue);

                accumulator = lastValue.Value;
            }

            var data = new long?[termCol.RowCount];
            for (int i = 0; i < termCol.RowCount; i++)
            {
                var restart = restartCol[i];
                if (restart == true)
                {
                    accumulator = 0;
                }

                var term = termCol[i];
                if (term.HasValue)
                {
                    accumulator += term.Value;
                }

                data[i] = accumulator;
            }

            return new ColumnarResult(new Column<long?>(ScalarTypes.Long, data));
        }
    }

    internal class RowCumSumDoubleFunctionImpl : IWindowFunctionImpl
    {
        public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments, ColumnarResult? previousWindowResult)
        {
            Debug.Assert(thisWindowArguments.Length == 2);
            var termCol = (Column<double?>)thisWindowArguments[0].Column;
            var restartCol = (Column<bool?>)thisWindowArguments[1].Column;

            double accumulator = 0;
            if (previousWindowResult != null)
            {
                var previousColumn = (Column<double?>)previousWindowResult.Column;
                Debug.Assert(previousColumn.RowCount > 0);

                var lastValue = previousColumn[previousColumn.RowCount - 1];
                Debug.Assert(lastValue.HasValue);

                accumulator = lastValue.Value;
            }

            var data = new double?[termCol.RowCount];
            for (int i = 0; i < termCol.RowCount; i++)
            {
                var restart = restartCol[i];
                if (restart == true)
                {
                    accumulator = 0;
                }

                var term = termCol[i];
                if (term.HasValue)
                {
                    accumulator += term.Value;
                }

                data[i] = accumulator;
            }

            return new ColumnarResult(new Column<double?>(ScalarTypes.Real, data));
        }
    }

    internal class RowCumSumTimeSpanFunctionImpl : IWindowFunctionImpl
    {
        public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments, ColumnarResult? previousWindowResult)
        {
            Debug.Assert(thisWindowArguments.Length == 2);
            var termCol = (Column<TimeSpan?>)thisWindowArguments[0].Column;
            var restartCol = (Column<bool?>)thisWindowArguments[1].Column;

            TimeSpan accumulator = TimeSpan.Zero;
            if (previousWindowResult != null)
            {
                var previousColumn = (Column<TimeSpan?>)previousWindowResult.Column;
                Debug.Assert(previousColumn.RowCount > 0);

                var lastValue = previousColumn[previousColumn.RowCount - 1];
                Debug.Assert(lastValue.HasValue);

                accumulator = lastValue.Value;
            }

            var data = new TimeSpan?[termCol.RowCount];
            for (int i = 0; i < termCol.RowCount; i++)
            {
                var restart = restartCol[i];
                if (restart == true)
                {
                    accumulator = TimeSpan.Zero;
                }

                var term = termCol[i];
                if (term.HasValue)
                {
                    accumulator += term.Value;
                }

                data[i] = accumulator;
            }

            return new ColumnarResult(new Column<TimeSpan?>(ScalarTypes.TimeSpan, data));
        }
    }
}
