using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Log10")]
internal partial class Log10Function
{
    private static double Impl(double input) => Math.Log10(input);
}