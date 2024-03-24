using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.EndsWithCs")]
internal partial class EndsWithCsFunction
{
    private static bool Impl(string a, string b) => a.EndsWith(b, StringComparison.InvariantCulture);
}