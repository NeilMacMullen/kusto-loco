using KustoLoco.Core;
namespace Lokql.Engine;

public interface ITableAdaptor : IKustoQueryContextTableLoader
{
    Task<bool> LoadTable(KustoQueryContext context, string path, string tableName);
    Task<bool> SaveResult(KustoQueryResult result, string path);
    IEnumerable<TableAdaptorDescription> GetSupportedAdaptors();

    void SetDataPaths(string path);
}
