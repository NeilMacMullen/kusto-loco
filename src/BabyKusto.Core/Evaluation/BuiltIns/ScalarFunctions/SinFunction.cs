using System;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class SinFunction
{
    private static double Impl(double input) => Math.Sin(input);
}