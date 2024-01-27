// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfDay")]
internal partial class StartOfDayFunction
{
    private static DateTime Impl(DateTime input) =>
        new(
            year: input.Year,
            month: input.Month,
            day: input.Day,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind);
}