// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class Log10Function
{
    private static double Impl(double input) => Math.Log10(input);
}