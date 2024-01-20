// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class AgoFunction
{
    private static DateTime Impl(TimeSpan t) => DateTime.UtcNow - t;
}