// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation]
    internal class ToStringFromIntFunction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Impl(int input)
            => input.ToString();
    }
}