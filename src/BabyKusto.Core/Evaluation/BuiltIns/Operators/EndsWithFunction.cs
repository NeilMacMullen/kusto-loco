﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.EndsWith")]
internal partial class EndsWithFunction
{
    private static bool Impl(string a, string b) => a.EndsWith(b, StringComparison.InvariantCultureIgnoreCase);
}