using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromRealFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(double input)
        => input.ToString();
}