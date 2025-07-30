// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOfbool)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOfbool)(arguments[2].Column);

        var data = NullableSetBuilderOfbool.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOfint)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOfint)(arguments[2].Column);

        var data = NullableSetBuilderOfint.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOflong)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOflong)(arguments[2].Column);

        var data = NullableSetBuilderOflong.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOfdouble)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOfdouble)(arguments[2].Column);

        var data = NullableSetBuilderOfdouble.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOfstring)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOfstring)(arguments[2].Column);

        var data = NullableSetBuilderOfstring.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOfDateTime)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOfDateTime)(arguments[2].Column);

        var data = NullableSetBuilderOfDateTime.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
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
        var predicateCol = (GenericTypedBaseColumnOfbool)(arguments[0].Column);
        var ifTrueCol = (GenericTypedBaseColumnOfTimeSpan)(arguments[1].Column);
        var ifFalseCol = (GenericTypedBaseColumnOfTimeSpan)(arguments[2].Column);

        var data = NullableSetBuilderOfTimeSpan.CreateFixed(predicateCol.RowCount);
        for (var i = 0; i < predicateCol.RowCount; i++)
        {
            var (ifTrue, ifFalse) = (ifTrueCol[i], ifFalseCol[i]);
            data.Add(predicateCol[i] == true ? ifTrue : ifFalse);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
    }
}
