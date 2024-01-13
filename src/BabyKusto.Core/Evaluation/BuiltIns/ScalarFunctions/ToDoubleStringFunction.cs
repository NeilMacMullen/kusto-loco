using System.Runtime.CompilerServices;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToDoubleStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double? Impl(string input) =>
        double.TryParse(input, out var parsedResult) ? parsedResult : null;
}