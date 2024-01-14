using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Exp")]
internal partial class ExpFunction
{
    private static double Impl(double input) => Math.Exp(input);
}