using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class GetYearFunction
{
    private static int Impl(DateTime date) => date.Year;
}