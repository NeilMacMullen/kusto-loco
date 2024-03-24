using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Round")]
internal partial class RoundFunction
{
    private static double Impl(double input, long precision) => Math.Round(input, (int)precision);
}