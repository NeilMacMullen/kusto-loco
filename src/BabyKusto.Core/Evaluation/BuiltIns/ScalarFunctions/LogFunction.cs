using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Log")]
internal partial class LogFunction
{
    private static double Impl(double input) => Math.Log(input);
}