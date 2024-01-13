using System;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class TanFunction
{
    private static double Impl(double input) => Math.Tan(input);
}