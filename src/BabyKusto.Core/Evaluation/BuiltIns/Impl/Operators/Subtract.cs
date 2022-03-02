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
            return new ScalarResult(ScalarTypes.Int, (int)arguments[0].Value - (int)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<int>)(arguments[0].Column);
            var right = (Column<int>)(arguments[1].Column);

            var data = new int[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] - right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class SubtractLongOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Long, (long)arguments[0].Value - (long)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<long>)(arguments[0].Column);
            var right = (Column<long>)(arguments[1].Column);

            var data = new long[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] - right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class SubtractDoubleOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Real, (double)arguments[0].Value - (double)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<double>)(arguments[0].Column);
            var right = (Column<double>)(arguments[1].Column);

            var data = new double[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] - right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }
    }

    internal class SubtractTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.TimeSpan, (TimeSpan)arguments[0].Value - (TimeSpan)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<TimeSpan>)(arguments[0].Column);
            var right = (Column<TimeSpan>)(arguments[1].Column);

            var data = new TimeSpan[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] - right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.TimeSpan, data));
        }
    }

    internal class SubtractDateTimeTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.DateTime, (DateTime)arguments[0].Value - (TimeSpan)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<DateTime>)(arguments[0].Column);
            var right = (Column<TimeSpan>)(arguments[1].Column);

            var data = new DateTime[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] - right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }
}
