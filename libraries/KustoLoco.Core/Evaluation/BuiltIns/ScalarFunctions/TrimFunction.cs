namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Trim")]
public partial class TrimFunction
{
    public string Impl(string pattern, string input)
    {
        return TrimHelpers.Evaluate(pattern, input, true, true);
    }
}
