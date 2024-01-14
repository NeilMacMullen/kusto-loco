using System.Runtime.CompilerServices;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToDoubleStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double? Impl(string input) =>
        double.TryParse(input, out var parsedResult) ? parsedResult : null;
}