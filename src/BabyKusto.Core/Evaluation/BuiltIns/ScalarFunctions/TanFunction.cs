using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Tan")]
internal partial class TanFunction
{
    private static double Impl(double input) => Math.Tan(input);
}