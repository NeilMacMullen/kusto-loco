// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Strlen")]
internal partial class StrlenFunction
{
    public long Impl(string s) => s.Length;
}