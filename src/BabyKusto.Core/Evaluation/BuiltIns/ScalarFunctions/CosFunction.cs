using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Cos")]
internal partial class CosFunction
{
    private static double Impl(double input) => Math.Cos(input);
}