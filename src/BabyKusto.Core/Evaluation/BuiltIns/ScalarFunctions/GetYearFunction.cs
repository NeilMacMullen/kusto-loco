using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class GetYearFunction
{
    private static long Impl(DateTime date) => date.Year;
}