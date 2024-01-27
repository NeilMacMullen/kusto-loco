using System;

// ReSharper disable PartialTypeWithSinglePart


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.MonthOfYear")]
internal partial class GetMonthFunction
{
    private static int Impl(DateTime date) => date.Month;
}