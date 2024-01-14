using System;
using System.Runtime.CompilerServices;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromTimeSpanFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(TimeSpan input)
        => input.ToString();
}