// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class BinIntFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (int?)arguments[0].Value;
            var right = (int?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.Int, Floor(left, right));
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
                data[i] = Floor(left, right);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int? Floor(int? left, int? right)
        {
            if (left.HasValue && right.HasValue)
            {
                if (right <= 0)
                {
                    // TODO: Should be null (perhaps?)
                    return 0;
                }
                else
                {
                    var remn = left.Value % right.Value;
                    if (remn < 0)
                    {
                        remn += right.Value;
                    }

                    return left.Value - remn;
                }
            }
            else
            {
                return null;
            }
        }
    }

    internal class BinLongFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (long?)arguments[0].Value;
            var right = (long?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.Long, Floor(left, right));
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
                data[i] = Floor(left, right);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long? Floor(long? left, long? right)
        {
            if (left.HasValue && right.HasValue)
            {
                if (right <= 0)
                {
                    // TODO: Should be null (perhaps?)
                    return 0;
                }
                else
                {
                    var remn = left.Value % right.Value;
                    if (remn < 0)
                    {
                        remn += right.Value;
                    }

                    return left.Value - remn;
                }
            }
            else
            {
                return null;
            }
        }
    }

    internal class BinDoubleFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (double?)arguments[0].Value;
            var right = (double?)arguments[1].Value;

            return new ScalarResult(ScalarTypes.Real, Floor(left, right));
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
                data[i] = Floor(left, right);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double? Floor(double? left, double? right)
        {
            if (left.HasValue && right.HasValue)
            {
                if (right <= 0)
                {
                    // TODO: Should be null (perhaps?)
                    return 0;
                }
                else
                {
                    return Math.Floor(left.Value / right.Value) * right.Value;
                }
            }
            else
            {
                return null;
            }
        }
    }

    internal class BinDateTimeTimeSpanFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var left = (DateTime?)arguments[0].Value;
            var right = (TimeSpan?)arguments[1].Value;
            var floor = BinLongFunctionImpl.Floor(left?.Ticks, right?.Ticks);
            return new ScalarResult(ScalarTypes.DateTime, floor.HasValue ? new DateTime(floor.Value) : null);
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
                var floor = BinLongFunctionImpl.Floor(left?.Ticks, right?.Ticks);
                data[i] = floor.HasValue ? new DateTime(floor.Value) : null;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }
}
