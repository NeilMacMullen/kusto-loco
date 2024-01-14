using System;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Abs")]
internal partial class AbsFunction
{
    private static double Impl(double number) => Math.Abs(number);
}