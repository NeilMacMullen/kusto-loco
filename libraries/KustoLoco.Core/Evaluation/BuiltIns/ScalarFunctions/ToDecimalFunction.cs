using System;
using System.Globalization;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToDecimal")]
internal partial class ToDecimalFunction
{
    private static decimal LongImpl(long input) => input;

    private static decimal DoubleImpl(double input) => (decimal)input;

    private static decimal? StringImpl(string input)
    {
        if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            input = input.Substring(2);

            return int.TryParse(input, NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture, out var x)
                ? x
                : null;
        }

        return decimal.TryParse(input, out var parsedResult)
            ? parsedResult
            : null;
    }
}
