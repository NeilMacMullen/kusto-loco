using System;
using System.Globalization;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToTimespan")]
internal partial class ToTimespanFunction
{
    private static TimeSpan? Impl(string input)
        => TimeSpan.TryParse(input, CultureInfo.InvariantCulture, out var result)
               ? result
               : null;
}
