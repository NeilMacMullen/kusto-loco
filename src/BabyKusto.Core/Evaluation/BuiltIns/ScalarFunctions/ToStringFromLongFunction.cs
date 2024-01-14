using System.Runtime.CompilerServices;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromLongFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(long input)
        => input.ToString();
}