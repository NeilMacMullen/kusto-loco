using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.GetYear")]
internal partial class GetYearFunction
{
    private static int Impl(DateTime date) => date.Year;
}