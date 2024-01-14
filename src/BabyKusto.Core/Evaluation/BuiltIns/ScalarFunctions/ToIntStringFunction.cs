using System.Runtime.CompilerServices;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToIntStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int? Impl(string input) =>
        int.TryParse(input, out var parsedResult)
            ? parsedResult
            : (double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) &&
               !double.IsInfinity(parsedDouble))
                ? (int)parsedDouble
                : null;
}