using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToDoubleStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double? Impl(string input) =>
        double.TryParse(input, out var parsedResult) ? parsedResult : null;
}