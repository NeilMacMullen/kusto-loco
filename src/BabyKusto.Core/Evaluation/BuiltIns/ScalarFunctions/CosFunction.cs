using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class CosFunction
{
    private static double Impl(double input) => Math.Cos(input);
}