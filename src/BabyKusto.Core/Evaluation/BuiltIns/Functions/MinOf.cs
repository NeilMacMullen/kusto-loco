// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class MinOfIntFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            int? min = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (int?)arguments[i].Value;
                if (item.HasValue && (!min.HasValue || item.Value < min.Value))
                {
                    min = item.Value;
                }
            }

            return new ScalarResult(ScalarTypes.Int, min);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new int?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                int? min = null;
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<int?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue && (!min.HasValue || item.Value < min.Value))
                    {
                        min = item.Value;
                    }
                }

                data[j] = min;
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class MinOfLongFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            long? min = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (long?)arguments[i].Value;
                if (item.HasValue && (!min.HasValue || item.Value < min.Value))
                {
                    min = item.Value;
                }
            }

            return new ScalarResult(ScalarTypes.Long, min);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new long?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                long? min = null;
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<long?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue && (!min.HasValue || item.Value < min.Value))
                    {
                        min = item.Value;
                    }
                }

                data[j] = min;
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class MinOfDoubleFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            double? min = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (double?)arguments[i].Value;
                if (item.HasValue && (!min.HasValue || item.Value < min.Value))
                {
                    min = item.Value;
                }
            }

            return new ScalarResult(ScalarTypes.Real, min);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new double?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                double? min = null;
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<double?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue && (!min.HasValue || item.Value < min.Value))
                    {
                        min = item.Value;
                    }
                }

                data[j] = min;
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }
    }
}
