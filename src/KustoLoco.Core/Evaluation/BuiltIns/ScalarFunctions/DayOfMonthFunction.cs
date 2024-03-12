using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DayOfMonth")]
internal partial class DayOfMonthFunction
{
    private static int Impl(DateTime input) => input.Day;
}