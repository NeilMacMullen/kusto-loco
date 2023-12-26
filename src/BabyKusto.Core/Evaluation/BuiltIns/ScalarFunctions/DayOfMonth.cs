// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class DayOfMonthFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var date = (DateTime?)arguments[0].Value;
        return new ScalarResult(ScalarTypes.Int, Impl(date));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var dates = (TypedBaseColumn<DateTime?>)arguments[0].Column;

        var data = new int?[dates.RowCount];
        for (var i = 0; i < dates.RowCount; i++)
        {
            data[i] = Impl(dates[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static int? Impl(DateTime? input)
    {
        if (input.HasValue)
        {
            return input.Value.Day;
        }

        return null;
    }
}