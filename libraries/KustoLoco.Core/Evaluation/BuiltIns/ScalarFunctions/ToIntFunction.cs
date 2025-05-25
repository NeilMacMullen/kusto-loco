using System;
using System.Globalization;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToInt")]
internal partial class ToIntFunction
{
    private static int DecImpl(decimal input) => (int)input;
    private static int IntImpl(int input) => input;
    private static int LongImpl(long input) => (int)input;
    private static int DoubleImpl(double input) => (int)input;

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
            : double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) &&
              !double.IsInfinity(parsedDouble)
                ? (int)parsedDouble
                : null;
    }
}
