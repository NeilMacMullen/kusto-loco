using System;

// ReSharper disable PartialTypeWithSinglePart


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Sin")]
internal partial class SinFunction
{
    private static double Impl(double input) => Math.Sin(input);
}