// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class EndOfMonthFunction
{
    private static DateTime Impl(DateTime input) =>
        new DateTime(
            year: input.Year,
            month: input.Month,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind).AddMonths(1).AddTicks(-1);
}