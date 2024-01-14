using System.Runtime.CompilerServices;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToBoolStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool? Impl(string input) =>
        bool.TryParse(input, out var parsedResult)
            ? parsedResult
            : long.TryParse(input, out var parsedLong)
                ? parsedLong != 0
                : null;
}