// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class PowFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var x = (double?)arguments[0].Value;
        var y = (double?)arguments[1].Value;

        return new ScalarResult(ScalarTypes.Real, Impl(x, y));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var xCol = (TypedBaseColumn<double?>)(arguments[0].Column);
        var yCol = (TypedBaseColumn<double?>)(arguments[1].Column);

        var data = new double?[xCol.RowCount];
        for (var i = 0; i < xCol.RowCount; i++)
        {
            data[i] = Impl(xCol[i], yCol[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static double? Impl(double? x, double? y)
    {
        if (x.HasValue && y.HasValue)
        {
            return Math.Pow(x.Value, y.Value);
        }

        return null;
    }
}