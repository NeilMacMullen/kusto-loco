using BabyKusto.Core;
using Kusto.Language.Symbols;

namespace KustoSupport;

/// <summary>
///     Simple abstraction of Kusto table
/// </summary>
/// <remarks>
///     Useful when building tables
/// </remarks>
public readonly record struct KustoTableDefinition(
    TableSymbol Symbol,
    IReadOnlyCollection<BaseColumn> Columns,
    int RowCount);