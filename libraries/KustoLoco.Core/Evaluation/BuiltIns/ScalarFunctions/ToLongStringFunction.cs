using System.Globalization;
using System;
using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToLongStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long? Impl(string input)
    {
        if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            input = input.Substring(2);

            return long.TryParse(input, NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture, out var x)
                ? x
                : null;
        }

        return long.TryParse(input, out var parsedResult)
            ? parsedResult
            : (double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) &&
               !double.IsInfinity(parsedDouble))
                ? (long)parsedDouble
                : null;
    }
}
