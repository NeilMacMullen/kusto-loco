using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Exp10")]
internal partial class Exp10Function
{
    private static double Impl(double input) => Math.Pow(input,10);
    private static double LongImpl(long input) => Math.Pow(input,10);
    private static double IntImpl(int input) => Math.Pow(input,10);
}
