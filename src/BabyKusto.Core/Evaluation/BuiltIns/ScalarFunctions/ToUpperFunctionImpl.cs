using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToUpper")]
public class ToUpperFunction
{
    private static string ToUpperImpl(string s) => s.ToUpperInvariant();
}