using System;
using System.Globalization;

// ReSharper disable PartialTypeWithSinglePart


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToDateTime")]
internal partial class ToDateTimeFunction
{
    private static DateTime? Impl(string input) =>
        DateTime.TryParse(input,
           CultureInfo.GetCultureInfo("UK"),
            out var result) ? result : null;
}
