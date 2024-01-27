// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfWeek")]
internal partial class StartOfWeekFunction
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