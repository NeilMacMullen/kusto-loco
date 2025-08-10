using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Exp")]
internal partial class ExpFunction
{
    private static double Impl(double input) => Math.Exp(input);
    private static double LongImpl(long input) => Math.Exp(input);
    private static double IntImpl(int input) => Math.Exp(input);

}
