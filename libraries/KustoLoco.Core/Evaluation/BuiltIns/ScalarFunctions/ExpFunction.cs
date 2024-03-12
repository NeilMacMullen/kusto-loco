using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Exp")]
internal partial class ExpFunction
{
    private static double Impl(double input) => Math.Exp(input);
}