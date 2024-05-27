

using KustoLoco.Core;

namespace KustoLoco.FileFormats;

/// <summary>
/// Describes an object that can Serialize a table from a file or other source
/// </summary>
/// <remarks>
/// Although the assumption here is that the table is being loaded from a *file*, other sources are possible.
/// KustoSettingsProvider are passed in to provide a generic mechanism for users to request behaviour such as automatic
/// type inference.
/// </remarks>
public interface ITableSerializer
{
    /// <summary>
    /// Attempts to load a table from a path
    /// </summary>
    Task<TableLoadResult> LoadTable(string path,string tableName);

    Task<TableLoadResult> LoadTable(Stream stream, string tableName);


    /// <summary>
    /// Attempts to save a table to a patch
    /// </summary>
    Task<TableSaveResult> SaveTable(string path,KustoQueryResult result);

    Task<TableSaveResult> SaveTable(Stream stream, KustoQueryResult result);
}
