

using KustoLoco.Core;

namespace KustoLoco.FileFormats;

/// <summary>
/// Describes an object that can load a table from a file or other source
/// </summary>
public interface ITableSerializer
{
    /// <summary>
    /// Attempts to load a table from a path
    /// </summary>
    Task<TableLoadResult> LoadTable(string path,string tableName,IProgress<string> progressReporter);
    /// <summary>
    /// Indicates that the serialization is insufficient to represent type information and that type inference is required
    /// </summary>
    public bool RequiresTypeInference { get; }

    Task<TableSaveResult> SaveTable(string path,KustoQueryResult result,  IProgress<string> progressReporter);
}



