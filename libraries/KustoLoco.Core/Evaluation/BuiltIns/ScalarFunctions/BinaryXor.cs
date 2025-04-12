namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "binary_xor")]
internal partial class BinaryXor
{
    private static long Impl(long a, long b)
    {
        return a ^ b;
    }
}
