// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class PowFunction
{
    private static double Impl(double x, double y) => Math.Pow(x, y);
}