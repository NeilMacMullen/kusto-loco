namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Strlen")]
internal partial class StrlenFunction
{
    public long Impl(string s) => s.Length;
}
