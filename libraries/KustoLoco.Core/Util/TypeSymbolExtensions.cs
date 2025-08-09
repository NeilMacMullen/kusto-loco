//
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Extensions;

public static class TypeSymbolExtensions
{
    [return: NotNullIfNotNull("type")]
    public static TypeSymbol? Simplify(this TypeSymbol? type) =>
        type == null ? null :
        type.Name == ScalarTypes.Dynamic.Name ? ScalarTypes.Dynamic :
        type;
}