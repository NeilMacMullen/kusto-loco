using BabyKusto.Core.Util;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
public partial class ToLowerFunctionImpl : IScalarFunctionImpl
{
    private static string ToLowerImpl(string s) => s.ToLowerInvariant();
}