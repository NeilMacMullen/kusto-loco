using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Round")]
internal partial class RoundFunction
{
    private static double DIImpl(double input, int precision) => Math.Round(input, precision);
    private static double DLImpl(double input, long precision) => Math.Round(input, (int)precision);
    private static decimal DecIntImpl(decimal input, int precision) => Math.Round(input,precision);
    private static decimal DecLongImpl(decimal input, long precision) => Math.Round(input, (int)precision);
    private static double Impl(double input) => Math.Round(input, 0);
    private static decimal DecImpl(decimal input) => Math.Round(input, 0);
}
