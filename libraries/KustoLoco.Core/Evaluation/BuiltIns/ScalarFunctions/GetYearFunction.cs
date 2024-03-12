using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.GetYear")]
internal partial class GetYearFunction
{
    private static int Impl(DateTime date) => date.Year;
}