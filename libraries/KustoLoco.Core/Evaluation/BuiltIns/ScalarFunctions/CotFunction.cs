using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Cot")]
internal partial class CotFunction
{
    private static double Impl(double input) => Math.Atan(input);
}

[KustoImplementation(Keyword = "Functions.Ceiling")]
internal partial class CeilingFunction
{
    private static double IntImpl(int input) => Math.Ceiling((double)input);
    private static double LongImpl(long input) => Math.Ceiling((double)input);
    private static double DecImpl(decimal input) => Math.Ceiling((double)input);

    private static double Impl(double input) => Math.Ceiling(input);
}
