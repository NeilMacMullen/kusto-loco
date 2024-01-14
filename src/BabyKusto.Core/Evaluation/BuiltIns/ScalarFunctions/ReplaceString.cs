// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ReplaceStringFunction
{
    private static string Impl(string text, string lookup, string rewrite)
        => text.Replace(lookup, rewrite);
}