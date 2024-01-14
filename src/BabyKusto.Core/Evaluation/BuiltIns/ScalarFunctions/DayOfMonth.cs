// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class DayOfMonthFunction
{
    private static int Impl(DateTime input) => input.Day;
}