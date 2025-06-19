namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToHex")]
internal partial class ToHexFunction
{
    private static string LongImpl(long input) => input.ToString("x");
    private static string Impl(int input) => input.ToString("x");
}
