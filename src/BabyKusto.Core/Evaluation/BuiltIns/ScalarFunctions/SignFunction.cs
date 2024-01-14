using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class SignFunction
{
    private static double Impl(double input) => Math.Sign(input);
}