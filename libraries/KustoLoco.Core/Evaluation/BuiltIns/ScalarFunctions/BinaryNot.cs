namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.BinaryNot")]
internal partial class BinaryNot
{
    private static long Impl(long a)
    {
        return ~a ;
    }
}
