﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.EqualTilde")]
internal partial class EqualTildeFunction
{
    private static bool StrImpl(string a, string b) => a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
}