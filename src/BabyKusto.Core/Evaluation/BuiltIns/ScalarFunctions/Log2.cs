// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;


namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class Log2Function
{
    private static double Impl(double input) =>
        Math.Log(input) / MathConstants.Log2;
}