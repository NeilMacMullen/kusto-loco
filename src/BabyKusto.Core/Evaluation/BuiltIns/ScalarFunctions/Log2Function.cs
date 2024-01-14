using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Log2")]
internal partial class Log2Function
{
    private static double Impl(double input) =>
        Math.Log(input) / MathConstants.Log2;
}