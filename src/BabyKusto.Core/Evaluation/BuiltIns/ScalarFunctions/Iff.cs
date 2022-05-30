// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class IffBoolFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (bool?)arguments[1].Value;
            var ifFalse = (bool?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.Bool, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<bool?>)(arguments[1].Column);
            var ifFalseCol = (Column<bool?>)(arguments[2].Column);

            var data = new bool?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }

    internal class IffIntFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (int?)arguments[1].Value;
            var ifFalse = (int?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.Int, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<int?>)(arguments[1].Column);
            var ifFalseCol = (Column<int?>)(arguments[2].Column);

            var data = new int?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }
    }

    internal class IffLongFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (long?)arguments[1].Value;
            var ifFalse = (long?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.Long, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<long?>)(arguments[1].Column);
            var ifFalseCol = (Column<long?>)(arguments[2].Column);

            var data = new long?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }

    internal class IffRealFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (double?)arguments[1].Value;
            var ifFalse = (double?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.Real, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<double?>)(arguments[1].Column);
            var ifFalseCol = (Column<double?>)(arguments[2].Column);

            var data = new double?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }
    }

    internal class IffStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (string?)arguments[1].Value;
            var ifFalse = (string?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.String, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<string?>)(arguments[1].Column);
            var ifFalseCol = (Column<string?>)(arguments[2].Column);

            var data = new string?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }
    }

    internal class IffDateTimeFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (DateTime?)arguments[1].Value;
            var ifFalse = (DateTime?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.DateTime, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<DateTime?>)(arguments[1].Column);
            var ifFalseCol = (Column<DateTime?>)(arguments[2].Column);

            var data = new DateTime?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }

    internal class IffTimeSpanFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var predicate = (bool?)arguments[0].Value;
            var ifTrue = (TimeSpan?)arguments[1].Value;
            var ifFalse = (TimeSpan?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.TimeSpan, predicate == true ? ifTrue : ifFalse);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var predicateCol = (Column<bool?>)(arguments[0].Column);
            var ifTrueCol = (Column<TimeSpan?>)(arguments[1].Column);
            var ifFalseCol = (Column<TimeSpan?>)(arguments[2].Column);

            var data = new TimeSpan?[predicateCol.RowCount];
            for (int i = 0; i < predicateCol.RowCount; i++)
            {
                var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
                data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.TimeSpan, data));
        }
    }
}
