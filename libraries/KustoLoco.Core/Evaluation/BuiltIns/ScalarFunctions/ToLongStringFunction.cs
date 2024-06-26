﻿using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

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