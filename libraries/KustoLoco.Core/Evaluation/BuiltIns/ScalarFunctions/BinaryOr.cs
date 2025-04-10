namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "binary_or")]
internal partial class BinaryOr
{
    private static long Impl(long a, long b)
    {
        return a | b;
    }
}
