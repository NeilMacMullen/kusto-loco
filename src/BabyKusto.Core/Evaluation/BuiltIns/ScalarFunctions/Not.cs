// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class NotFunction
{
    internal static bool Impl(bool v) => !v;
}