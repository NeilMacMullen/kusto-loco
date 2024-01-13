using System;
using System.Runtime.CompilerServices;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromTimeSpanFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(TimeSpan input)
        => input.ToString();
}