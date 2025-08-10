using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.Contains")]
internal partial class ContainsFunction
{
    private static bool Impl(string a, string b) => a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
}


[KustoImplementation(Keyword = "Operators.Has")]
internal partial class HasFunction
{
    private static bool Impl(string a, string b) => a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
}

