// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class CoalesceBoolFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            bool? result = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (bool?)arguments[i].Value;
                if (item.HasValue)
                {
                    result = item.Value;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.Bool, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new bool?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<bool?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue)
                    {
                        data[j] = item.Value;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class CoalesceIntFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            int? result = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (int?)arguments[i].Value;
                if (item.HasValue)
                {
                    result = item.Value;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.Int, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new int?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<int?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue)
                    {
                        data[j] = item.Value;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class CoalesceLongFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            long? result = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (long?)arguments[i].Value;
                if (item.HasValue)
                {
                    result = item.Value;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.Long, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new long?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<long?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue)
                    {
                        data[j] = item.Value;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class CoalesceDoubleFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            double? result = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (double?)arguments[i].Value;
                if (item.HasValue)
                {
                    result = item.Value;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.Real, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new double?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<double?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue)
                    {
                        data[j] = item.Value;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }
    }

    internal class CoalesceDateTimeFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            DateTime? result = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (DateTime?)arguments[i].Value;
                if (item.HasValue)
                {
                    result = item.Value;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.DateTime, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new DateTime?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<DateTime?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue)
                    {
                        data[j] = item.Value;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }

    internal class CoalesceTimeSpanFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            TimeSpan? result = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (TimeSpan?)arguments[i].Value;
                if (item.HasValue)
                {
                    result = item.Value;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.TimeSpan, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new TimeSpan?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<TimeSpan?>)arguments[i].Column;
                    var item = column[j];
                    if (item.HasValue)
                    {
                        data[j] = item.Value;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.TimeSpan, data));
        }
    }

    internal class CoalesceStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            string? result = string.Empty;
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = (string?)arguments[i].Value;
                if (!string.IsNullOrEmpty(item))
                {
                    result = item;
                    break;
                }
            }

            return new ScalarResult(ScalarTypes.String, result);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length > 0);

            int numRows = arguments[0].Column.RowCount;
            var data = new string?[numRows];
            for (int j = 0; j < numRows; j++)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    var column = (Column<string?>)arguments[i].Column;
                    var item = column[j];
                    if (!string.IsNullOrEmpty(item))
                    {
                        data[j] = item;
                        break;
                    }
                }
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }
    }
}
