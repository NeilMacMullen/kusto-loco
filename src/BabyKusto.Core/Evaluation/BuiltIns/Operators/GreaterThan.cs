// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class GreaterThanIntOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (int?)arguments[0].Value;
            var right = (int?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left > right : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<int?>)(arguments[0].Column);
            var rightCol = (Column<int?>)(arguments[1].Column);

            var data = new bool?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left > right : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class GreaterThanLongOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (long?)arguments[0].Value;
            var right = (long?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left > right : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<long?>)(arguments[0].Column);
            var rightCol = (Column<long?>)(arguments[1].Column);

            var data = new bool?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left > right : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class GreaterThanDoubleOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (double?)arguments[0].Value;
            var right = (double?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left > right : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<double?>)(arguments[0].Column);
            var rightCol = (Column<double?>)(arguments[1].Column);

            var data = new bool?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left > right : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class GreaterThanTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (TimeSpan?)arguments[0].Value;
            var right = (TimeSpan?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left > right : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<TimeSpan?>)(arguments[0].Column);
            var rightCol = (Column<TimeSpan?>)(arguments[1].Column);

            var data = new bool?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left > right : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class GreaterThanDateTimeOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (DateTime?)arguments[0].Value;
            var right = (DateTime?)arguments[1].Value;
            return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left > right : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var leftCol = (Column<DateTime?>)(arguments[0].Column);
            var rightCol = (Column<DateTime?>)(arguments[1].Column);

            var data = new bool?[leftCol.RowCount];
            for (int i = 0; i < leftCol.RowCount; i++)
            {
                var (left, right) = (leftCol[i], rightCol[i]);
                data[i] = left.HasValue && right.HasValue ? left > right : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }
}
