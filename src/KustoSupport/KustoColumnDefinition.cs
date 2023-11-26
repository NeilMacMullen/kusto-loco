using Kusto.Language.Symbols;

namespace KustoSupport;

/// <summary>
///     An abstraction of the Kusto Column Definition
/// </summary>
/// <remarks>
///     Kusto table population is a bit awkward so this enables us to define the column name, Kusto Type and
///     object accessor method in one hit.
/// </remarks>
public readonly record struct KustoColumnDefinition<T>(string Name, ScalarSymbol Type, Func<T, object> Value);
