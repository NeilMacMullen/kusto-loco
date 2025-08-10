using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Exp2")]
internal partial class Exp2Function
{
    private static double Impl(double input) => Math.Pow(input, 2);
    private static double LongImpl(long input) => Math.Pow(input, 2);
    private static double IntImpl(int input) => Math.Pow(input, 2);
}
