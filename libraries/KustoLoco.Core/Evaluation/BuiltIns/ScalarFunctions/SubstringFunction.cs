using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Substring")]
internal partial class SubstringFunction
{
    internal static string Impl(string input, long start, long length)
    {
        var effectiveStart = Math.Max(0, Math.Min(start, input.Length));
        var maxAllowableLength = input.Length - effectiveStart;
        var effectiveLength =
            Math.Max(0, Math.Min(length, maxAllowableLength));
        return input.Substring((int)effectiveStart, (int)effectiveLength);
    }

    private static string AImpl(string input, long start)
    {
        return SubstringFunction.Impl(input, start, int.MaxValue);
    }
}
