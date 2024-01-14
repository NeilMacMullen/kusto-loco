using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class RoundFunction
{
    private static double Impl(double input, long precision) => Math.Round(input, (int)precision);
}