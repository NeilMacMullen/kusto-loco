namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "trimws")]
internal partial class TrimWsFunction
{
    private static string Impl(string s) => s.Trim();
}