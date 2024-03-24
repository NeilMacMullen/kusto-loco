using KustoLoco.Core;

#pragma warning disable CS8618, CS8604
public interface ITableSerializer : IKustoQueryContextTableLoader
{
    Task<bool> LoadTable(KustoQueryContext context, string path, string tableName);
    Task SaveResult(KustoQueryResult result, string path);

}