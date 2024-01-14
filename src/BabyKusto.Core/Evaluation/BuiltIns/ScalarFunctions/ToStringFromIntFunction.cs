// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation]
    internal class ToStringFromIntFunction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Impl(int input)
            => input.ToString();
    }
}