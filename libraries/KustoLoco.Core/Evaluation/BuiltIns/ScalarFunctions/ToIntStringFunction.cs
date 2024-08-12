using System.Globalization;
using System;
using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToIntStringFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int? Impl(string input)
    {
        if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            input = input.Substring(2);

            return int.TryParse(input, NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture, out var x)
                ? x
                : null;
        }
        return int.TryParse(input, out var parsedResult)
            ? parsedResult
            : (double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) &&
               !double.IsInfinity(parsedDouble))
                ? (int)parsedDouble
                : null;
    }
}
