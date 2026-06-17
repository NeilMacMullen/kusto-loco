using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfYear")]
internal partial class StartOfYearFunction
{

    internal static DateTime 
    Calculate(DateTime input,int yearsOffset) =>
        new DateTime(
            year: input.Year,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind)
            .AddYears(yearsOffset);

    private static DateTime Impl(DateTime input) =>
        StartOfYearFunction.Calculate(input, 0);

    private static DateTime IntImpl(DateTime input,int offset) =>
        StartOfYearFunction.Calculate(input, offset);

    private static DateTime LongImpl(DateTime input, long offset) =>
        StartOfYearFunction.Calculate(input, (int) offset);

}
