namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.BinaryShiftRight")]
internal partial class BinaryShiftRight
{
    private static long Impl(long a, long b)
    {
        return a >> (int)b;
    }
}
