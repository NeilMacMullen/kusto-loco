using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class DegreesFunction
{
    private static double Impl(double radians) => radians * 180.0 / Math.PI;
}