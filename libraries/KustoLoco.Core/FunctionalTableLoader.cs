using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KustoLoco.Core;


/// <summary>
///     Implementation of table loader to perform easy adaptation between Functional and Interface style
/// </summary>
public class FunctionalTableLoader : IKustoQueryContextTableLoader
{
    private readonly Action<KustoQueryContext, IReadOnlyCollection<string>> _action;
    public FunctionalTableLoader(Action<KustoQueryContext, IReadOnlyCollection<string>> action) => _action = action;

    public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
    {
        _action(context, tableNames);
        return Task.CompletedTask;
    }

    public Task<bool> LoadTable(KustoQueryContext context, string path, string tableName)
        => throw new NotImplementedException();

    public Task SaveResult(KustoQueryResult result, string path) => throw new NotImplementedException();
}
