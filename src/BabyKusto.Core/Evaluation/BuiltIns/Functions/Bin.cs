// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class BinIntFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (int)arguments[0].Value;
            var right = (int)arguments[1].Value;

            int floor;
            if (right <= 0)
            {
                // TODO: Should be null (perhaps?)
                floor = 0;
            }
            else
            {
                int remn = left % right;
                if (remn < 0)
                {
                    remn += right;
                }

                floor = left - remn;
            }

            return new ScalarResult(ScalarTypes.Int, floor);
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
                int floor;
                if (right[i] <= 0)
                {
                    // TODO: Should be null (perhaps?)
                    floor = 0;
                }
                else
                {
                    int remn = left[i] % right[i];
                    if (remn < 0)
                    {
                        remn += right[i];
                    }

                    floor = left[i] - remn;
                }

                data[i] = floor;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class BinLongFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (long)arguments[0].Value;
            var right = (long)arguments[1].Value;

            long floor;
            if (right <= 0)
            {
                // TODO: Should be null (perhaps?)
                floor = 0;
            }
            else
            {
                long remn = left % right;
                if (remn < 0)
                {
                    remn += right;
                }

                floor = left - remn;
            }

            return new ScalarResult(ScalarTypes.Long, floor);
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
                long floor;
                if (right[i] <= 0)
                {
                    // TODO: Should be null (perhaps?)
                    floor = 0;
                }
                else
                {
                    long remn = left[i] % right[i];
                    if (remn < 0)
                    {
                        remn += right[i];
                    }

                    floor = left[i] - remn;
                }

                data[i] = floor;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class BinDateTimeTimeSpanFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (DateTime)arguments[0].Value;
            var right = (TimeSpan)arguments[1].Value;

            long floor;
            if (right.Ticks <= 0)
            {
                // TODO: Should be null (perhaps?)
                floor = 0;
            }
            else
            {
                long remn = left.Ticks % right.Ticks;
                if (remn < 0)
                {
                    remn += right.Ticks;
                }

                floor = left.Ticks - remn;
            }

            return new ScalarResult(ScalarTypes.DateTime, new DateTime(floor));
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
                long floor;
                if (right[i].Ticks <= 0)
                {
                    // TODO: Should be null (perhaps?)
                    floor = 0;
                }
                else
                {
                    long remn = left[i].Ticks % right[i].Ticks;
                    if (remn < 0)
                    {
                        remn += right[i].Ticks;
                    }

                    floor = left[i].Ticks - remn;
                }

                data[i] = new DateTime(floor);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }
}
