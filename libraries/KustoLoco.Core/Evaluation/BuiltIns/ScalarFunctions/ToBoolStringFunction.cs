using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

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