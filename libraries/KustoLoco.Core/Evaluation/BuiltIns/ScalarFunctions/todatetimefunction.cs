using System;
using System.Globalization;

// ReSharper disable PartialTypeWithSinglePart


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToDateTime")]
internal partial class ToDateTimeFunction
{
    private static DateTime? Impl(string input) =>
        DateTime.TryParse(input, CultureInfo.GetCultureInfo("en-GB"),
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var result) ? result : null;
}
