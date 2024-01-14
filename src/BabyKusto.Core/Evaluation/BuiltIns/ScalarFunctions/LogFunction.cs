using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Log")]
internal partial class LogFunction
{
    private static double Impl(double input) => Math.Log(input);
}