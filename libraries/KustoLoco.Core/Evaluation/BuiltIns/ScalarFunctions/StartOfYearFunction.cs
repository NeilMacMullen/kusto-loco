﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfYear")]
internal partial class StartOfYearFunction
{
    private static DateTime Impl(DateTime input) =>
        new(
            year: input.Year,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind);
}