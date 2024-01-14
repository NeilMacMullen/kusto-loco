using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class GetMonthFunction
{
    private static int Impl(DateTime date) => date.Month;
}