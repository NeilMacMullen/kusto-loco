// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core.Util;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal partial class StrlenFunctionImpl : IScalarFunctionImpl
{
    public long Impl(string s) => s.Length;
}