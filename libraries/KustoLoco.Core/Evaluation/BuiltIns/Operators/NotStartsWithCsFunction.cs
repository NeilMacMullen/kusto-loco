﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotStartsWithCs")]
internal partial class NotStartsWithCsFunction
{
    private static bool Impl(string a, string b) => !a.StartsWith(b, StringComparison.InvariantCulture);
}