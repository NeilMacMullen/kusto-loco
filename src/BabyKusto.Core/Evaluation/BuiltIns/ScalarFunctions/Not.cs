// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class NotFunction
{
    internal static bool Impl(bool v) => !v;
}