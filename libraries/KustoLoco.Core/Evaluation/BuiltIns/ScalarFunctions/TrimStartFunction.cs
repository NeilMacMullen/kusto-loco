namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.TrimStart")]
public partial class TrimStartFunction
{
    public string Impl(string pattern, string input)
    {
        return TrimHelpers.Evaluate(pattern, input, true, false);
    }
}
