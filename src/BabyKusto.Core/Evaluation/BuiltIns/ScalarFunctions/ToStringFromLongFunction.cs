using System.Runtime.CompilerServices;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromLongFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(long input)
        => input.ToString();
}