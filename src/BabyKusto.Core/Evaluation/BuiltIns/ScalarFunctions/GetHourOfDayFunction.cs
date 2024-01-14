using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class GetHourOfDayFunction
{
    private static long Impl(DateTime date) => date.Hour;
}