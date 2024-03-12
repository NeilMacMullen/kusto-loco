using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Sign")]
internal partial class SignFunction
{
    private static double Impl(double input) => Math.Sign(input);
}