// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class UnaryMinusIntOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            return new ScalarResult(ScalarTypes.Int, -(int?)arguments[0].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int?>)(arguments[0].Column);

            var data = new int?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = -column[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class UnaryMinusLongOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            return new ScalarResult(ScalarTypes.Long, -(long?)arguments[0].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long?>)(arguments[0].Column);

            var data = new long?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = -column[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class UnaryMinusDoubleOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            return new ScalarResult(ScalarTypes.Real, -(double?)arguments[0].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double?>)(arguments[0].Column);

            var data = new double?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = -column[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }
    }

    internal class UnaryMinusTimeSpanOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            return new ScalarResult(ScalarTypes.TimeSpan, -(TimeSpan?)arguments[0].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<TimeSpan?>)(arguments[0].Column);

            var data = new TimeSpan?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = -column[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.TimeSpan, data));
        }
    }
}
