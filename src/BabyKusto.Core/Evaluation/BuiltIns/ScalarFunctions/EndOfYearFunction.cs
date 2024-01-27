using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.EndOfYear")]
internal partial class EndOfYearFunction
{
    private static DateTime Impl(DateTime input) =>
        new DateTime(
            year: input.Year,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind).AddYears(1).AddTicks(-1);
}