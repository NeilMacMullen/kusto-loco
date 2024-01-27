using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfMonth")]
internal partial class StartOfMonthFunction
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