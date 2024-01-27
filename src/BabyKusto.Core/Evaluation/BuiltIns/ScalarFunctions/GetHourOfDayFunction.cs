using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.HourOfDay")]
internal partial class GetHourOfDayFunction
{
    private static long Impl(DateTime date) => date.Hour;
}