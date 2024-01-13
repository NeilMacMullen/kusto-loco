// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class IsEmptyFunction
{
    internal static bool Impl(string s) => s.Length == 0;
}