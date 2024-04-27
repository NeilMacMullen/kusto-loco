using KustoLoco.Core;
using KustoLoco.Core.DataSource;

namespace KustoLoco.FileFormats;

/// <summary>
/// Represents the result of a table load operation - Table will be TableSource.Empty if the load failed
/// </summary>
public readonly record struct TableLoadResult(ITableSource Table, string Error)
{
    public static TableLoadResult Failure(string error) => new(NullTableSource.Instance, error);
    public static TableLoadResult Success(ITableSource table) => new(table, string.Empty);
}
