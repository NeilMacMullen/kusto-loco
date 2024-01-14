using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class RadiansFunction
{
    private static double Impl(double degrees) => Math.PI / 180 * degrees;
}