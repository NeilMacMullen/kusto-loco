using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class TrimWsFunction
{
    private static string Impl(string s) => s.Trim();
}