namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.BinaryOr")]
internal partial class BinaryOr
{
    private static long Impl(long a, long b)
    {
        return a | b;
    }
}
