using System;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Abs")]
internal partial class AbsFunction
{
    private static long IntImpl(int number) => Math.Abs(number);
    private static long LongImpl(long number) => Math.Abs(number);
    private static double DoubleImpl(double number) => Math.Abs(number);
   
}