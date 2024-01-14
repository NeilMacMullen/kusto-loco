// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class SubstringFunction
{
    private static string Impl(string input, long start, long length)
    {
        var effectiveStart = Math.Max(0, Math.Min(start, input.Length));
        var maxAllowableLength = input.Length - effectiveStart;
        var effectiveLength =
            Math.Max(0, Math.Min(length, maxAllowableLength));
        return input.Substring((int)effectiveStart, (int)effectiveLength);
    }
}