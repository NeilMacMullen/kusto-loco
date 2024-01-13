using System.Runtime.CompilerServices;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromRealFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(double input)
        => input.ToString();
}