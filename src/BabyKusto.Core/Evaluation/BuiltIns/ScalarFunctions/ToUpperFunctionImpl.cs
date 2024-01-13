using BabyKusto.Core.Util;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
public partial class ToUpperFunctionImpl : IScalarFunctionImpl
{
    private static string ToUpperImpl(string s) => s.ToUpperInvariant();
}