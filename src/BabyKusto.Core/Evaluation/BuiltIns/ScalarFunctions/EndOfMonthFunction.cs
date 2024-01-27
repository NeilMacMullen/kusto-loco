using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.EndOfMonth")]
internal partial class EndOfMonthFunction
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