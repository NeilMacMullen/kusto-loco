using System.Runtime.CompilerServices;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToLongStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long? Impl(string input) =>
        long.TryParse(input, out var parsedResult)
            ? parsedResult
            : (double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) &&
               !double.IsInfinity(parsedDouble))
                ? (long)parsedDouble
                : null;
}