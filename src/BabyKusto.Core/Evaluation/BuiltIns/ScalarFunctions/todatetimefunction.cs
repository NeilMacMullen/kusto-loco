using System;

// ReSharper disable PartialTypeWithSinglePart


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToDateTime")]
internal partial class ToDateTimeFunction
{
    private static DateTime? Impl(string input) => DateTime.TryParse(input, out var result) ? result : null;
}