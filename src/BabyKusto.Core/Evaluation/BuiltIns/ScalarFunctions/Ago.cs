// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class AgoFunction
{
    private static TimeSpan Impl(DateTime t) => DateTime.UtcNow - t;
}