// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class StartOfMonthFunction
{
    private static DateTime Impl(DateTime input) =>
        new(
            year: input.Year,
            month: input.Month,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind);
}