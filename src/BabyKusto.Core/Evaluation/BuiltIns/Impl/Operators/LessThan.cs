// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class LessThanIntOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Bool, (int)arguments[0].Value < (int)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<int>)(arguments[0].Column);
            var right = (Column<int>)(arguments[1].Column);

            var data = new bool[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] < right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class LessThanLongOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Bool, (long)arguments[0].Value < (long)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<long>)(arguments[0].Column);
            var right = (Column<long>)(arguments[1].Column);

            var data = new bool[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] < right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class LessThanDoubleOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Bool, (double)arguments[0].Value < (double)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<double>)(arguments[0].Column);
            var right = (Column<double>)(arguments[1].Column);

            var data = new bool[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] < right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class LessThanTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Bool, (TimeSpan)arguments[0].Value < (TimeSpan)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<TimeSpan>)(arguments[0].Column);
            var right = (Column<TimeSpan>)(arguments[1].Column);

            var data = new bool[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] < right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class LessThanDateTimeOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Bool, (DateTime)arguments[0].Value < (DateTime)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<DateTime>)(arguments[0].Column);
            var right = (Column<DateTime>)(arguments[1].Column);

            var data = new bool[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] < right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }
}
