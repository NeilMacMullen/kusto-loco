using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.Contains")]
internal partial class ContainsFunction
{
    private static bool Impl(ContainsContext context, string a, string b)
    {
        if (context.LastA == a && context.LastB == b) return context.LastResult;

        var result = a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
        context.LastA = a;
        context.LastB = b;
        context.LastResult = result;
        return result;
    }
}

public class ContainsContext
{
    public string LastA = string.Empty;
    public string LastB = string.Empty;
    public bool LastResult;
}