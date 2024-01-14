using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
public class ToLowerFunction
{
    private static string ToLowerImpl(string s) => s.ToLowerInvariant();
}