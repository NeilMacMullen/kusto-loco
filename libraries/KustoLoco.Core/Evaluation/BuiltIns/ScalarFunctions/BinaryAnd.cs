namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "binary_and")]
internal partial class BinaryAnd
{
    private static long Impl(long a,long b)
    {
        return a & b;
    }
}
