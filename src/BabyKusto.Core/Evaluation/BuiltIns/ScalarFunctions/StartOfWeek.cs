// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class StartOfWeekFunction
{
    private static DateTime Impl(DateTime input)
    {
        var startOfDay = new DateTime(
            year: input.Year,
            month: input.Month,
            day: input.Day,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind);
        var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
        return startOfWeek;
    }
}