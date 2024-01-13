// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class EndOfDayFunction
{
    private static DateTime Impl(DateTime input) =>
        new DateTime(
            year: input.Year,
            month: input.Month,
            day: input.Day,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind).AddDays(1).AddTicks(-1);
}