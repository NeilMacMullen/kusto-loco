namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToUpper")]
public class ToUpperFunction
{
    private static string Impl(string s) => s.ToUpperInvariant();
}