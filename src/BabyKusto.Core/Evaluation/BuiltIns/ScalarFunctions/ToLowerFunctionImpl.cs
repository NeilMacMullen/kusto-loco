namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
public class ToLowerFunction
{
    private static string Impl(string s) => s.ToLowerInvariant();
}