﻿using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.GreaterThan")]
internal partial class GreaterThanFunction
{
    private static bool IntImpl(int a, int b) => a > b;
    private static bool LongImpl(long a, long b) => a > b;

    private static bool DoubleImpl(double a, double b) => a > b;
    private static bool TsImpl(TimeSpan a, TimeSpan b) => a > b;
    private static bool DtImpl(DateTime a, DateTime b) => a > b;
}