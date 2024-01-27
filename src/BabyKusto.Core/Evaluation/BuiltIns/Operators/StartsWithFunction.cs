﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.StartsWith")]
internal partial class StartsWithFunction
{
    private static bool Impl(string a, string b) => a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase);
}