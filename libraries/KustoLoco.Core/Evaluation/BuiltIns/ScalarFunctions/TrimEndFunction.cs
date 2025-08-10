namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.TrimEnd")]
public partial class TrimEndFunction
{
    public string Impl(string pattern, string input)
    {
        return TrimHelpers.Evaluate(pattern, input, false, true);
    }
}
