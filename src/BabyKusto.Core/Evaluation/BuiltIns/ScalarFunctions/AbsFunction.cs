using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class AbsFunction
{
    private static double Impl(double number) => Math.Abs(number);
}