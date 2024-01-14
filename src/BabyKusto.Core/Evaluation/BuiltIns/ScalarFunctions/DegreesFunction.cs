using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Degrees")]
internal partial class DegreesFunction
{
    private static double Impl(double radians) => radians * 180.0 / Math.PI;
}