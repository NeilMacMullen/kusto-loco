using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromDecimalFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(decimal input)
        => input.ToString();
}
