// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class SubtractIntOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (int?)arguments[0].Value;
            var right = (int?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Int, left.HasValue && right.HasValue ? left.Value - right.Value : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<int?>)(arguments[0].Column);
            var rightCol = (Column<int?>)(arguments[1].Column);

            var data = new int?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left.Value - right.Value : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class SubtractLongOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (long?)arguments[0].Value;
            var right = (long?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.Long, left.HasValue && right.HasValue ? left.Value - right.Value : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<long?>)(arguments[0].Column);
            var rightCol = (Column<long?>)(arguments[1].Column);

            var data = new long?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left.Value - right.Value : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class SubtractDoubleOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (double?)arguments[0].Value;
            var right = (double?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.Real, left.HasValue && right.HasValue ? left.Value - right.Value : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<double?>)(arguments[0].Column);
            var rightCol = (Column<double?>)(arguments[1].Column);

            var data = new double?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left.Value - right.Value : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }
    }

    internal class SubtractTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (TimeSpan?)arguments[0].Value;
            var right = (TimeSpan?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.TimeSpan, left.HasValue && right.HasValue ? new TimeSpan(left.Value.Ticks - right.Value.Ticks) : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<TimeSpan?>)(arguments[0].Column);
            var rightCol = (Column<TimeSpan?>)(arguments[1].Column);

            var data = new TimeSpan?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? new TimeSpan(left.Value.Ticks - right.Value.Ticks) : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.TimeSpan, data));
        }
    }

    internal class SubtractDateTimeTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (DateTime?)arguments[0].Value;
            var right = (TimeSpan?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.DateTime, left.HasValue && right.HasValue ? left.Value - right.Value : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<DateTime?>)(arguments[0].Column);
            var rightCol = (Column<TimeSpan?>)(arguments[1].Column);

            var data = new DateTime?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left.Value - right.Value : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }
}
