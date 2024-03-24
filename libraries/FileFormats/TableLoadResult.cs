using KustoLoco.Core;

namespace KustoLoco.FileFormats;

/// <summary>
/// Represents the result of a table load operation - Table will be TableSource.Empty if the load failed
/// </summary>
public readonly record struct TableLoadResult(ITableSource Table, string Error);


#pragma warning disable CS8602, CS8604, CS8600