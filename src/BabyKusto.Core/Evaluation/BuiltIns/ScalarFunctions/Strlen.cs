// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class StrlenFunction
{
    public long Impl(string s) => s.Length;
}