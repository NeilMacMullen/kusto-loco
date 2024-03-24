using System;

// ReSharper disable PartialTypeWithSinglePart


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Sin")]
internal partial class SinFunction
{
    private static double Impl(double input) => Math.Sin(input);
}