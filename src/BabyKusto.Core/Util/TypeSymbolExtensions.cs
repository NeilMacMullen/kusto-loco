// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.Extensions
{
    internal static class TypeSymbolExtensions
    {
#if NETSTANDARD2_1_OR_GREATER
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("type")]
#endif
        internal static TypeSymbol? Simplify(this TypeSymbol? type)
        {
            return
                type == null ? null :
                type.Name == ScalarTypes.Dynamic.Name ? ScalarTypes.Dynamic :
                type;
        }
    }
}
