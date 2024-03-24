using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KustoLoco.Core;


/// <summary>
///     Default implementation of table loader that doesn't actually load any tables
/// </summary>
internal class NullTableLoader : IKustoQueryContextTableLoader
{
    public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
        => Task.CompletedTask;

    public Task<bool> LoadTable(KustoQueryContext context, string path, string tableName)
        => throw new NotImplementedException();

    public Task SaveResult(KustoQueryResult result, string path) => throw new NotImplementedException();
}