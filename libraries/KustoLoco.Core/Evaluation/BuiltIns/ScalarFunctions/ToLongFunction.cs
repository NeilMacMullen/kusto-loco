using System;
using System.Globalization;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToLong")]
internal partial class ToLongFunction
{
    private static long DecImpl(decimal input) => (long)input;
    private static long IntImpl(int input) => input;
    private static long LongImpl(long input) => input;
    private static long DoubleImpl(double input) => (long)input;

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
            : double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) &&
              !double.IsInfinity(parsedDouble)
                ? (long)parsedDouble
                : null;
    }
}
