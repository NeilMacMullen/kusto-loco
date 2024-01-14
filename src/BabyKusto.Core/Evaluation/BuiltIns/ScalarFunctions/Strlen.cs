// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class StrlenFunction
{
    public long Impl(string s) => s.Length;
}