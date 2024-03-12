// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ReplaceString")]
internal partial class ReplaceStringFunction
{
    private static string Impl(string text, string lookup, string rewrite)
        => text.Replace(lookup, rewrite);
}