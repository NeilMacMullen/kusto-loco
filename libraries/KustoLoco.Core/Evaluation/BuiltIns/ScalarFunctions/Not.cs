// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Not")]
internal partial class NotFunction
{
    internal static bool Impl(bool v) => !v;
}