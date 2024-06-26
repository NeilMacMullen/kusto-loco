﻿using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DayOfYear")]
internal partial class DayOfYearFunction
{
    private static int Impl(DateTime input)
    {
        var startOfYear = new DateTime(
            year: input.Year,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind);
        return (int)(input - startOfYear).TotalDays + 1;
    }
}