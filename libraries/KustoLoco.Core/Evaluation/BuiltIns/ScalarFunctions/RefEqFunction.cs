namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "refequal")]
internal partial class RefEqFunction
{
    private static bool Impl(string a,string b) => ReferenceEquals(a,b);
}
