namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "binary_shift_right")]
internal partial class BinaryShiftRight
{
    private static long Impl(long a, long b)
    {
        return a >> (int)b;
    }
}
