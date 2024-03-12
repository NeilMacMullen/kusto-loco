using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Pow")]
internal partial class PowFunction
{
    private static double Impl(double x, double y) => Math.Pow(x, y);
}