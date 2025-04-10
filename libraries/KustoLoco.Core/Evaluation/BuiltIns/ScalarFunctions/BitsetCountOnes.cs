namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "bitset_count_ones")]
internal partial class BitsetCountOnes
{
    private static long Impl(long a)
    {
        var x = (ulong)a;
        var n = 0;
        while (x > 0)
        {
            n++;
            x &= x - 1;
        }
        return n;
    }
}
