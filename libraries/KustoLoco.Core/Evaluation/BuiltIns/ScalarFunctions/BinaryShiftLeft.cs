namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.BinaryShiftLeft")]
internal partial class BinaryShiftLeft
{
    private static long Impl(long a,long b)
    {
        return a<<(int)b;
    }
}
