// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DayOfMonth")]
internal partial class DayOfMonthFunction
{
    private static int Impl(DateTime input) => input.Day;
}