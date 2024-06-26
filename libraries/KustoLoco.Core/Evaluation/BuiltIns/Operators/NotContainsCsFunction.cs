﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotContainsCs")]
internal partial class NotContainsCsFunction
{
    private static bool Impl(string a, string b) => !a.Contains(b, StringComparison.InvariantCulture);
}