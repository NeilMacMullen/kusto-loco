// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class DayOfYearFunctionImpl : IScalarFunctionImpl
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
        var dates = (Column<DateTime?>)arguments[0].Column;

        var data = new int?[dates.RowCount];
        for (var i = 0; i < dates.RowCount; i++)
        {
            data[i] = Impl(dates[i]);
        }

        return new ColumnarResult(BaseColumn.Create(ScalarTypes.Int, data));
    }

    private static int? Impl(DateTime? input)
    {
        if (input.HasValue)
        {
            var startOfYear = new DateTime(
                year: input.Value.Year,
                month: 1,
                day: 1,
                hour: 0,
                minute: 0,
                second: 0,
                kind: input.Value.Kind);
            return (int)(input.Value - startOfYear).TotalDays + 1;
        }

        return null;
    }
}