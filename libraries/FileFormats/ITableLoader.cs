namespace KustoLoco.FileFormats;

public interface ITableLoader
{
    /// <summary>
    /// Loads a table from a path
    /// </summary>
    Task<TableLoadResult> LoadTable(string path,string tableName,IProgress<string> progressReporter);
    /// <summary>
    /// Indicates that the serialization is insufficient to represent type information and that type inference is required
    /// </summary>
    public bool RequiresTypeInference { get; }
}