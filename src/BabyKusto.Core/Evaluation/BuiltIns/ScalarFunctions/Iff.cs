// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<bool?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<bool?>)(arguments[2].Column);

        var data = new bool?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<int?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<int?>)(arguments[2].Column);

        var data = new int?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Int, data));
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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<long?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<long?>)(arguments[2].Column);

        var data = new long?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Long, data));
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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<double?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<double?>)(arguments[2].Column);

        var data = new double?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Real, data));
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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<string?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<string?>)(arguments[2].Column);

        var data = new string?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.String, data));
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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<DateTime?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<DateTime?>)(arguments[2].Column);

        var data = new DateTime?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.DateTime, data));
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
        var predicateCol = (TypedBaseColumn<bool?>)(arguments[0].Column);
        var ifTrueCol = (TypedBaseColumn<TimeSpan?>)(arguments[1].Column);
        var ifFalseCol = (TypedBaseColumn<TimeSpan?>)(arguments[2].Column);

        var data = new TimeSpan?[predicateCol.RowCount];
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data[i] = predicateCol[i] == true ? ifTrue : ifFalse;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.TimeSpan, data));
    }
}