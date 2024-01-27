using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.ContainsCs")]
internal partial class ContainsCsFunction
{
    private static bool Impl(string a, string b) => a.Contains(b, StringComparison.InvariantCulture);
}