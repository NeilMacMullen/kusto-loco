using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Cos")]
internal partial class CosFunction
{
    private static double Impl(double input) => Math.Cos(input);
}
