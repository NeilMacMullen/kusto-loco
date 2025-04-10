namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "binary_not")]
internal partial class BinaryNot
{
    private static long Impl(long a)
    {
        return ~a ;
    }
}
