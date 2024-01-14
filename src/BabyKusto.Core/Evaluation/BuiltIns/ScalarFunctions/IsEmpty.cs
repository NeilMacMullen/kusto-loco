// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class IsEmptyFunction
{
    internal static bool Impl(string s) => s.Length == 0;
}