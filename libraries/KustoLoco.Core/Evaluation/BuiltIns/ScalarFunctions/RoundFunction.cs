using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Round")]
internal partial class RoundFunction
{
    private static double ImplP(double input, long precision) => Math.Round(input, (int)precision);
    private static double Impl(double input) => Math.Round(input, 0);
}
